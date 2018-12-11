using UnityEngine;

namespace LunraGames.SubLight
{
	public class OpacityAnimation : ViewAnimation
	{
		[SerializeField]
		AnimationCurve showingOpacity = AnimationCurveExtensions.Constant(1f);
		[SerializeField]
		AnimationCurve closingOpacity = AnimationCurveExtensions.Constant(1f);


		public override void OnShowing(IView view, float scalar)
		{
			Debug.LogError("not implimented");
			//view.Opacity = showingOpacity.Evaluate(scalar);
		}

		public override void OnClosing(IView view, float scalar)
		{
			Debug.LogError("not implimented");
			//view.Opacity = closingOpacity.Evaluate(scalar);
		}
	}
}