using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

namespace LunraGames.SubLight 
{
	
	public enum TransitionStates
	{
		Unknown = 0,
		Shown = 10,
		Showing = 20,
		Closed = 30,
		Closing = 40
	}

	public interface IView : IMonoBehaviour
	{
		Transform Parent { get; set; }
		Transform Root { get; }

		float ShowDuration { get; }
		float CloseDuration { get; }
		float Progress { get; set; }

		void PushOpacity(Func<float> query);
		void PopOpacity(Func<float> query);
		void ClearOpacity();
		void SetOpacityStale();

		float OpacityStack { get; }

		float Opacity { get; set; }
		bool Interactable { get; set; }
		int PoolSize { get; }

		TransitionStates TransitionState { get; }

		bool Visible { get; }
		/// <summary>
		/// Called when view is prepared. Add events using += for predictable behaviour.
		/// </summary>
		/// <value>The prepare.</value>
		Action Prepare { get; set; }
		/// <summary>
		/// Called when view is showing, with a scalar progress. Add events using += for predictable behaviour.
		/// </summary>
		/// <value>The showing.</value>
		Action<float> Showing { get; set; }
		/// <summary>
		/// Called when view is shown. Add events using += for predictable behaviour.
		/// </summary>
		/// <value>The shown.</value>
		Action Shown { get; set; }
		/// <summary>
		/// Called when view is idle, with a delta in seconds since the last call. Add events using += for predictable behaviour.
		/// </summary>
		/// <value>The idle.</value>
		Action<float> Idle { get; set; }
		/// <summary>
		/// Called on view late idle, with a delta in seconds since the lats call. Add events using += for predictable behaviour.
		/// </summary>
		/// <value>The late idle.</value>
		Action<float> LateIdle { get; set; }
		/// <summary>
		/// Called when a view starts to close, only once at the beginning.
		/// </summary>
		/// <value>The prepare close.</value>
		Action PrepareClose { get; set; }
		/// <summary>
		/// Called when view is closing, with a scalar progress. Add events using += for predictable behaviour.
		/// </summary>
		/// <value>The closing.</value>
		Action<float> Closing { get; set; }
		/// <summary>
		/// Called when view is closed. Add events using += for predictable behaviour.
		/// </summary>
		/// <value>The closed.</value>
		Action Closed { get; set; }

		void Reset();

		string InstanceName { get; set; }

		void SetLayer(string layer);
	}

	public abstract class View : MonoBehaviour, IView
	{
		const float ShowDurationDefault = 0.2f;
		const float CloseDurationDefault = 0.2f;

		public Transform Parent { get; set; }

		public virtual Transform Root { get { return transform; } }

		public virtual float ShowDuration { get { return ShowCloseDuration.OverrideShow ? ShowCloseDuration.ShowDuration : ShowDurationDefault; } }
		public virtual float CloseDuration { get { return ShowCloseDuration.OverrideClose ? ShowCloseDuration.CloseDuration : CloseDurationDefault; } }
		public virtual float Progress { get; set; }

		TransitionStates transitionState;
		public TransitionStates TransitionState
		{
			get { return transitionState == TransitionStates.Unknown ? TransitionStates.Closed : transitionState; }
			protected set { transitionState = value; }
		}

		bool opacityStackStale;
		float lastCalculatedOpacityStack = 1f;
		List<Func<float>> opacityStack = new List<Func<float>>();
		public void PushOpacity(Func<float> query) { opacityStack.Remove(query); opacityStack.Add(query); SetOpacityStale(); }
		public void PopOpacity(Func<float> query) { opacityStack.Remove(query); SetOpacityStale(); }
		public void ClearOpacity() { opacityStack.Clear(); SetOpacityStale(); }
		public void SetOpacityStale() { opacityStackStale = true; }

		public float OpacityStack { get { return lastCalculatedOpacityStack; } }

		void CheckOpacityStack()
		{
			if (!opacityStackStale) return;
			if (CalculateOpacityStack()) OnOpacityStack(OpacityStack);
		}

		bool CalculateOpacityStack()
		{
			var original = lastCalculatedOpacityStack;
			var opacity = 1f;
			foreach (var entry in opacityStack)
			{
				opacity *= entry();
				if (Mathf.Approximately(opacity, 0f)) break;
			}
			lastCalculatedOpacityStack = opacity;
			return Mathf.Approximately(original, lastCalculatedOpacityStack);
		}

		protected virtual void OnOpacityStack(float opacity) {}

		float opacity = 1f;
		public virtual float Opacity { get { return opacity; } set { opacity = Mathf.Max(0f, Mathf.Min(1f, value)); } }
		bool interactable = true;
		public virtual bool Interactable { get { return interactable; } set { interactable = value; } }
		[SerializeField, Tooltip("Size of initial pool, entering \"0\" uses ViewMediator defaults.")]
		int poolSize;
		public virtual int PoolSize { get { return poolSize; } }
		public ShowCloseDurationBlock ShowCloseDuration;
		[SerializeField, FormerlySerializedAs("_animations")]
		ViewAnimation[] animations;
		public virtual ViewAnimation[] ViewAnimations { get { return animations; } }

		public string InstanceName 
		{
			get { return gameObject.name; }
			set { gameObject.name = value; }
		}

		public Action Prepare { get; set; }
		public Action<float> Showing { get; set; }
		public Action Shown { get; set; }
		public Action<float> Idle { get; set; }
		public Action<float> LateIdle { get; set; }
		public Action PrepareClose { get; set; }
		public Action<float> Closing { get; set; }
		public Action Closed { get; set; }

		protected virtual void OnPrepare()
		{
			TransitionState = TransitionStates.Showing;

			Root.SetParent(Parent, true);
			Root.localPosition = Vector3.zero;
			Root.localScale = Vector3.one;
			Root.localRotation = Quaternion.identity;
			Root.gameObject.SetActive(true);

			foreach (var anim in ViewAnimations) anim.OnPrepare(this);
			CheckOpacityStack();
		}

		protected virtual void OnShowing(float scalar) 
		{
			foreach (var anim in ViewAnimations) anim.OnShowing(this, scalar);
			CheckOpacityStack();
		}

		protected virtual void OnShown() 
		{
			TransitionState = TransitionStates.Shown;
			foreach (var anim in ViewAnimations) anim.OnShown(this);
			CheckOpacityStack();
		}

		protected virtual void OnIdle(float delta) 
		{
			foreach (var anim in ViewAnimations) anim.OnIdle(this, delta);
			CheckOpacityStack();
		}

		protected virtual void OnLateIdle(float delta)
		{
			foreach (var anim in ViewAnimations) anim.OnLateIdle(this, delta);
			CheckOpacityStack();
		}

		protected virtual void OnPrepareClose()
		{
			TransitionState = TransitionStates.Closing;
			foreach (var anim in ViewAnimations) anim.OnPrepareClose(this);
			CheckOpacityStack();
		}

		protected virtual void OnClosing(float scalar) 
		{
			foreach (var anim in ViewAnimations) anim.OnClosing(this, scalar);
			CheckOpacityStack();
		}

		protected virtual void OnClosed() 
		{
			TransitionState = TransitionStates.Closed;
			foreach (var anim in ViewAnimations) anim.OnClosed(this);
			CheckOpacityStack();
		}

		public virtual void Reset() 
		{
			Prepare = OnPrepare;
			Shown = OnShown;
			Showing = OnShowing;
			Idle = OnIdle;
			LateIdle = OnLateIdle;
			PrepareClose = OnPrepareClose;
			Closing = OnClosing;
			Closed = OnClosed;

			ClearOpacity();
			Opacity = 1f;
			Interactable = true;
		}

		public bool Visible { get { return TransitionState != TransitionStates.Closed; } }

		public void SetLayer(string layer)
		{
			Root.gameObject.SetLayerRecursively(LayerMask.NameToLayer(layer));
		}
	}
}
