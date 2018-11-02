using UnityEngine;

namespace LunraGames.SubLight
{
	public class PitchOpacityAnimation : ViewAnimation
	{
		[SerializeField]
		CurveStyleBlock showingOpacity = CurveStyleBlock.Default;
		[SerializeField]
		CurveStyleBlock closingOpacity = CurveStyleBlock.Default;
		[SerializeField]
		CurveStyleBlock pitchOpacity = CurveStyleBlock.Default;
		[SerializeField]
		float hideRevealDuration;
		[SerializeField]
		CurveStyleBlock hideRevealCurve = CurveStyleBlock.Default;

		long lastFrame;
		float lastMoveOpacity;
		float elapsed;

		float GetMoveOpacity(float delta)
		{
			if (lastFrame != App.V.FrameCount)
			{
				lastFrame = App.V.FrameCount;

				if (App.V.CameraHasMoved) elapsed = Mathf.Min(hideRevealDuration, elapsed + delta);
				else elapsed = Mathf.Max(0f, elapsed - delta);

				lastMoveOpacity = hideRevealCurve.Evaluate(1f - (elapsed / hideRevealDuration));
			}
			return lastMoveOpacity;
		}

		float GetOpacity(float pitch)
		{
			return pitchOpacity.Evaluate(pitch);
		}

		float GetOpacity(float pitch, float scalar, AnimationCurve showCloseOpacity)
		{
			return GetOpacity(pitch) * showCloseOpacity.Evaluate(scalar);
		}

		public override void OnShowing(IView view, float scalar)
		{
			view.Opacity = GetOpacity(App.V.CameraTransform.PitchValue(), scalar, showingOpacity);
		}

		public override void OnIdle(IView view, float delta)
		{
			view.Opacity = GetMoveOpacity(delta) * GetOpacity(App.V.CameraTransform.PitchValue());
		}

		public override void OnClosing(IView view, float scalar)
		{
			view.Opacity = GetOpacity(App.V.CameraTransform.PitchValue(), scalar, closingOpacity);
		}
	}
}