using UnityEngine;

namespace LunraGames.SubLight
{
	public class PitchOpacityAnimation : ViewAnimation
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		CurveStyleBlock pitchOpacity = CurveStyleBlock.Default;
		[SerializeField]
		float hideRevealDuration;
		[SerializeField]
		CurveStyleBlock hideRevealCurve = CurveStyleBlock.Default;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		float elapsed;

		float lastOpacity;
		bool hasChanged;

		public override void OnPrepare(IView view)
		{
			view.PushOpacity(() => lastOpacity);
		}

		public override void OnConstantOnce(IView view, float delta)
		{
			var original = lastOpacity;

			if (App.V.CameraHasMoved) elapsed = Mathf.Min(hideRevealDuration, elapsed + delta);
			else elapsed = Mathf.Max(0f, elapsed - delta);

			lastOpacity = hideRevealCurve.Evaluate(1f - (elapsed / hideRevealDuration));
			lastOpacity *= pitchOpacity.Evaluate(App.V.CameraTransform.PitchValue());

			hasChanged = !Mathf.Approximately(original, lastOpacity);
		}

		public override void OnConstant(IView view, float delta)
		{
			if (hasChanged) view.SetOpacityStale();
		}
	}
}