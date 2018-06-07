using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

using LunraGames;

namespace LunraGames.SpaceFarm.Views
{
	public class ReticleView : View, IReticleView
	{
		struct StateEntry
		{
			public ReticleStates State;
			public ReticleBlock Block;
			public int? PlayCount;
			public Action Done;

			public StateEntry(ReticleBlock block, int? playCount, Action done)
			{
				State = block == null ? ReticleStates.Unknown : block.State;
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
		Transform reticlesRoot;
		[SerializeField, FormerlySerializedAs("Reticles")]
		ReticleBlock[] reticles = new ReticleBlock[0];

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
		public void Push(ReticleStates state, int? playCount = null, bool reset = false, Action done = null)
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
			if (block.State == ReticleStates.Unknown) 
			{
				Debug.LogError("No ReticleBlock for state "+state+" found");
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

		void ToggleBlock(ReticleBlock block, bool value)
		{
			foreach (var blockObject in block.Objects) blockObject.SetActive(value);
		}

		StateEntry GetEntry(ReticleStates state, int? playCount, Action done)
		{
			return new StateEntry(reticles.FirstOrDefault(r => r.State == state), playCount, done);
		}

		public override void Reset()
		{
			base.Reset();

			//Position = Vector3.zero;
			//Rotation = Quaternion.identity;
			Pop(true);

			foreach (var block in reticles) ToggleBlock(block, false);
		}

		void SetDilation(RangeResult range, Color color, ReticleBlock target)
		{
			color = color * this.color;
			foreach (var reticle in target.Reticles)
			{
				reticle.material.SetColor(ShaderConstants.Reticle.Color, color);
			}
		}

		void LateUpdate()
		{
			//reticlesRoot.position = Position;
			//reticlesRoot.rotation = Rotation;

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

		//void OnTransition(float delta)
		//{
		//	loops = 0;
		//	transitionProgress = Mathf.Min(TransitionDuration, transitionProgress + delta);
		//	transitioning = !Mathf.Approximately(TransitionDuration, transitionProgress);
		//	if (!transitioning)
		//	{
		//		transitionProgress = 0f;
		//		animationProgress = 0f;
		//		return;
		//	}
		//	var currentRange = current.Animation.Dilation.Evaluate(0f);
		//	var minDelta = currentRange.Min - lastRange.Min;
		//	var maxDelta = currentRange.Max - lastRange.Max;
		//	var transitionScalar = transitionProgress / transitionDuration;

		//	var result = new RangeResult(
		//		lastRange.Min + (minDelta * transitionScalar),
		//		lastRange.Max + (maxDelta * transitionScalar),
		//		0f
		//	);

		//	SetDilation(result, current);
		//}

		//void OnAnimate(float delta)
		//{
		//	var newProgress = animationProgress + delta;
		//	if (AnimationDuration <= newProgress) loops++;
		//	animationProgress = newProgress % AnimationDuration;
		//	var animationScalar = animationProgress / AnimationDuration;
		//	var currentResult = current.Animation.Dilation.Evaluate(animationScalar);
		//	SetDilation(currentResult, current, currentResult);

		//	if (nextState != ReticleStates.Unknown && nextState != current.State) SetState(nextState);
		//}

	}

	public enum ReticleStates
	{
		Unknown,
		Idle,
		Highlight,
		Click
	}

	public interface IReticleView : IView
	{
		//Vector3 Position { set; }
		//Quaternion Rotation { set; }
		void Push(ReticleStates state, int? playCount = null, bool reset = false, Action done = null);
	}

	[Serializable]
	public class ReticleBlock
	{
		public ReticleStates State;
		public GameObject[] Objects = new GameObject[0];
		public MeshRenderer[] Reticles = new MeshRenderer[0];
		public ReticleAnimation Animation;
	}
}