using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public class ReticleAnimation : ScriptableObject
	{
		public bool PlayAtLeastOnce;
		public bool OverrideTransitionDuration;
		public float TransitionDuration;
		public bool OverrideAnimationDuration;
		public float AnimationDuration;
		public RangeCurve Dilation;
		public Gradient Gradient;

		public float GetTransitionDuration(float defaultDuration)
		{
			return OverrideTransitionDuration ? TransitionDuration : defaultDuration;
		}

		public float GetAnimationDuration(float defaultDuration)
		{
			return OverrideAnimationDuration ? AnimationDuration : defaultDuration;
		}

		public float GetTransitionScalar(float progress, float defaultDuration)
		{
			return progress / GetTransitionDuration(defaultDuration);
		}

		public float GetAnimationScalar(float progress, float defaultDuration)
		{
			return progress / GetAnimationDuration(defaultDuration);
		}
	}
}