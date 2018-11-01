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

		float GetOpacity(float pitch)
		{
			return pitchOpacity.Curve.Evaluate(pitch);
		}

		float GetOpacity(float pitch, float scalar, AnimationCurve showCloseOpacity)
		{
			return GetOpacity(pitch) * showCloseOpacity.Evaluate(scalar);
		}

		public override void OnShowing(IView view, float scalar)
		{
			view.Opacity = GetOpacity(App.V.CameraTransform.PitchValue(), scalar, showingOpacity);
		}

		public override void OnIdle(IView view)
		{
			view.Opacity = GetOpacity(App.V.CameraTransform.PitchValue());
		}

		public override void OnClosing(IView view, float scalar)
		{
			view.Opacity = GetOpacity(App.V.CameraTransform.PitchValue(), scalar, closingOpacity);
		}
	}
}