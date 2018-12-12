﻿using UnityEngine;

namespace LunraGames.SubLight
{
	public class PitchOpacityAnimation : ViewAnimation
	{
		[SerializeField]
		CurveStyleBlock pitchOpacity = CurveStyleBlock.Default;
		[SerializeField]
		float hideRevealDuration;
		[SerializeField]
		CurveStyleBlock hideRevealCurve = CurveStyleBlock.Default;

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