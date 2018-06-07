using UnityEngine;

using LunraGames;

namespace LunraGames.SpaceFarm
{
	public class OpacityAnimation : ViewAnimation
	{
		[SerializeField]
		AnimationCurve showingOpacity = AnimationCurveExtensions.Constant(1f);
		[SerializeField]
		AnimationCurve closingOpacity = AnimationCurveExtensions.Constant(1f);

		public override void OnShowing(IView view, float scalar)
		{
			view.Opacity = showingOpacity.Evaluate(scalar);
		}

		public override void OnClosing(IView view, float scalar)
		{
			view.Opacity = closingOpacity.Evaluate(scalar);
		}
	}
}