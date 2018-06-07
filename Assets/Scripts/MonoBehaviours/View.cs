using UnityEngine;
using System;

namespace LunraGames.SpaceFarm 
{
	
	public enum TransitionStates
	{
		UNKNOWN,
		SHOWN,
		SHOWING,
		CLOSED,
		CLOSING
	}

	public interface IView : IMonoBehaviour
	{
		Transform Parent { get; set; }
		Transform Root { get; }

		float ShowDuration { get; }
		float CloseDuration { get; }
		float Progress { get; set; }

		float Opacity { get; set; }
		bool Interactable { get; set; }

		TransitionStates TransitionState { get; }

		bool Visible { get; }
		Action Prepare { get; set; }
		Action<float> Showing { get; set; }
		Action Shown { get; set; }
		Action<float> Idle { get; set; }
		Action<float> LateIdle { get; set; }
		Action<float> Closing { get; set; }
		Action Closed { get; set; }
		void Reset();

		string InstanceName { get; set; }	
	}

	public abstract class View : MonoBehaviour, IView
	{
		const float ShowDurationDefault = 0.2f;
		const float CloseDurationDefault = 0.2f;

		public Transform Parent { get; set; }

		public virtual Transform Root { get { return transform; } }

		public virtual float ShowDuration { get { return ShowDurationDefault; } }
		public virtual float CloseDuration { get { return CloseDurationDefault; } }
		public virtual float Progress { get; set; }

		public TransitionStates TransitionState { get; protected set; }

		float opacity = 1f;
		public virtual float Opacity { get { return opacity; } set { opacity = Mathf.Max(0f, Mathf.Min(1f, value)); } }
		bool _interactable = true;
		public virtual bool Interactable { get { return _interactable; } set { _interactable = value; } }
		[SerializeField]
		ViewAnimation[] _animations;

		public virtual ViewAnimation[] ViewAnimations { get { return _animations; } }

		public string InstanceName {
			get { return gameObject.name; }
			set { gameObject.name = value; }
		}

		public Action Prepare { get; set; }
		public Action<float> Showing { get; set; }
		public Action Shown { get; set; }
		public Action<float> Idle { get; set; }
		public Action<float> LateIdle { get; set; }
		public Action<float> Closing { get; set; }
		public Action Closed { get; set; }

		protected virtual void OnPrepare()
		{
			TransitionState = TransitionStates.SHOWING;

			Root.SetParent(Parent, true);
			Root.localPosition = Vector3.zero;
			Root.localScale = Vector3.one;
			Root.localRotation = Quaternion.identity;
			Root.gameObject.SetActive(true);

			foreach (var anim in ViewAnimations) anim.OnPrepare(this);
		}

		protected virtual void OnShowing(float scalar) 
		{
			TransitionState = TransitionStates.SHOWING;
			foreach (var anim in ViewAnimations) anim.OnShowing(this, scalar);
		}

		protected virtual void OnShown() 
		{
			TransitionState = TransitionStates.SHOWN;
			foreach (var anim in ViewAnimations) anim.OnShown(this);
			//App.Heartbeat.Update += OnUpdate;
			//App.Heartbeat.LateUpdate += OnLateUpdate;
		}

		protected virtual void OnIdle(float delta) 
		{
			foreach (var anim in ViewAnimations) anim.OnIdle(this);
		}

		protected virtual void OnLateIdle(float delta)
		{
			foreach (var anim in ViewAnimations) anim.OnLateIdle(this);
		}

		protected virtual void OnClosing(float scalar) 
		{
			// Is this the first time coming through here...
			if (TransitionState != TransitionStates.CLOSING)
			{
				//App.Heartbeat.Update -= OnUpdate;
				//App.Heartbeat.LateUpdate -= OnLateUpdate;
			}
			TransitionState = TransitionStates.CLOSING;
			foreach (var anim in ViewAnimations) anim.OnClosing(this, scalar);
		}

		protected virtual void OnClosed() 
		{
			TransitionState = TransitionStates.CLOSED;
			foreach (var anim in ViewAnimations) anim.OnClosed(this);
		}

		public virtual void Reset() 
		{
			Prepare = OnPrepare;
			Shown = OnShown;
			Showing = OnShowing;
			Idle = OnIdle;
			LateIdle = OnLateIdle;
			Closing = OnClosing;
			Closed = OnClosed;

			Opacity = 1f;
			Interactable = true;
		}

		public bool Visible { get { return gameObject.activeInHierarchy; } }
	}
}
