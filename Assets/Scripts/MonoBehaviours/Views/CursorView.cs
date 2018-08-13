using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

using LunraGames;

namespace LunraGames.SubLight.Views
{
	public class CursorView : View, ICursorView
	{
		struct StateEntry
		{
			public CursorStates State;
			public CursorBlock Block;
			public int? PlayCount;
			public Action Done;

			public StateEntry(CursorBlock block, int? playCount, Action done)
			{
				State = block == null ? CursorStates.Unknown : block.State;
				Block = block;
				PlayCount = playCount;
				Done = done;
			}
		}

		[SerializeField]
		Color color = Color.white;
		[SerializeField]
		float transitionDurationDefault;
		[SerializeField]
		float animationDurationDefault;
		[SerializeField]
		Transform cursorsRoot;
		[SerializeField]
		CursorBlock[] cursors = new CursorBlock[0];

		float animationProgress;
		float transitionProgress;
		List<StateEntry> stateStack = new List<StateEntry>();

		//public Vector3 Position { set; private get; }
		//public Quaternion Rotation { set; private get; }

		/// <summary>
		/// Push the specified state, playCount and done.
		/// </summary>
		/// <remarks>
		/// Warning: Done is only run if the state actually gets popped eventually by a succesful transition. Interrupts
		/// do not trigger a done!
		/// </remarks>
		/// <returns>The push.</returns>
		/// <param name="state">State.</param>
		/// <param name="playCount">Play count.</param>
		/// <param name="done">Done.</param>
		public void Push(CursorStates state, int? playCount = null, bool reset = false, Action done = null)
		{
			if (1 == stateStack.Count && reset && stateStack[0].State == state)
			{
				animationProgress = 0f;
				Pop(true, false);
			}
			else if (2 == stateStack.Count)
			{
				stateStack.RemoveAt(1);
			}

			var block = GetEntry(state, playCount, done);
			if (block.State == CursorStates.Unknown) 
			{
				Debug.LogError("No CursorBlock for state "+state+" found");
				return;
			}
			stateStack.Add(block);

			if (stateStack.Count == 1) ToggleBlock(block.Block, true);
		}

		void Pop(bool all = false, bool triggerDone = true)
		{
			if (all)
			{
				foreach (var entry in stateStack) ToggleBlock(entry.Block, false);
				stateStack.Clear();
				return;
			}
			if (0 < stateStack.Count)
			{
				var entry = stateStack[0];
				ToggleBlock(entry.Block, false);
				stateStack.RemoveAt(0);
				if (triggerDone && entry.Done != null) entry.Done();
			}
		}

		void ToggleBlock(CursorBlock block, bool value)
		{
			foreach (var blockObject in block.Objects) blockObject.SetActive(value);
		}

		StateEntry GetEntry(CursorStates state, int? playCount, Action done)
		{
			return new StateEntry(cursors.FirstOrDefault(r => r.State == state), playCount, done);
		}

		public override void Reset()
		{
			base.Reset();

			//Position = Vector3.zero;
			//Rotation = Quaternion.identity;
			Pop(true);

			foreach (var block in cursors) ToggleBlock(block, false);
		}

		void SetDilation(RangeResult range, Color color, CursorBlock target)
		{
			color = color * this.color;
			foreach (var cursor in target.Cursors)
			{
				cursor.material.SetColor(ShaderConstants.Cursor.Color, color);
			}
		}

		void LateUpdate()
		{
			// TODO: Delete these?
			//cursorsRoot.position = Position;
			//cursorsRoot.rotation = Rotation;

			if (stateStack.Count == 0) return;

			var isTransitioning = 1 < stateStack.Count;

			if (isTransitioning) OnTransition(Time.deltaTime);
			else OnAnimation(Time.deltaTime);
		}

		void OnTransition(float delta)
		{
			var current = stateStack[0].Block;
			var next = stateStack[1].Block;

			var duration = next.Animation.GetTransitionDuration(transitionDurationDefault);
			transitionProgress = Mathf.Min(duration, transitionProgress + delta);
			var done = Mathf.Approximately(duration, transitionProgress);
			if (done)
			{
				OnFinishTransition();
				return;
			}
			var fromScalar = current.Animation.GetAnimationScalar(animationProgress, animationDurationDefault);

			var fromRange = current.Animation.Dilation.Evaluate(fromScalar);
			var toRange = next.Animation.Dilation.Evaluate(0f);
			var minDelta = toRange.Min - fromRange.Min;
			var maxDelta = toRange.Max - fromRange.Max;

			var fromColor = current.Animation.Gradient.Evaluate(fromScalar);
			var toColor = next.Animation.Gradient.Evaluate(0f);

			if (fromColor.Approximately(toColor) && Mathf.Approximately(0f, minDelta) && Mathf.Approximately(0f, maxDelta))
			{
				OnFinishTransition();
				return;
			}

			var transitionScalar = next.Animation.GetTransitionScalar(transitionProgress, transitionDurationDefault);

			var dilation = new RangeResult(
				fromRange.Min + (minDelta * transitionScalar),
				fromRange.Max + (maxDelta * transitionScalar),
				0f
			);

			var color = Color.Lerp(fromColor, toColor, transitionScalar);

			SetDilation(dilation, color, current);
		}

		void OnFinishTransition()
		{
			animationProgress = 0f;
			transitionProgress = 0f;
			Pop();
			if (stateStack.Count == 0) return;
			var block = stateStack[0].Block;
			SetDilation(block.Animation.Dilation.Evaluate(0f), block.Animation.Gradient.Evaluate(0f), block);
			ToggleBlock(block, true);
		}

		void OnAnimation(float delta)
		{
			transitionProgress = 0f;
			var entry = stateStack[0];
			var current = entry.Block;
			var duration = current.Animation.GetAnimationDuration(animationDurationDefault);
			animationProgress = animationProgress + delta;
			if (entry.PlayCount.HasValue && duration <= animationProgress)
			{
				entry.PlayCount--;

				if (entry.PlayCount == 0)
				{
					Pop();
					return;
				}
			}
			animationProgress = animationProgress % duration;
			var scalar = current.Animation.GetAnimationScalar(animationProgress, animationDurationDefault);
			var dilation = current.Animation.Dilation.Evaluate(scalar);
			var color = current.Animation.Gradient.Evaluate(scalar);
			SetDilation(dilation, color, current);
		}
	}

	public enum CursorStates
	{
		Unknown,
		Idle,
		Highlight,
		Click
	}

	public interface ICursorView : IView
	{
		//Vector3 Position { set; }
		//Quaternion Rotation { set; }
		void Push(CursorStates state, int? playCount = null, bool reset = false, Action done = null);
	}

	[Serializable]
	public class CursorBlock
	{
		public CursorStates State;
		public GameObject[] Objects = new GameObject[0];
		public MeshRenderer[] Cursors = new MeshRenderer[0];
		public CursorAnimation Animation;
	}
}