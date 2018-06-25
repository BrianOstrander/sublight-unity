using UnityEngine;

using LunraGames;

namespace LunraGames.SpaceFarm
{
	public class LookScaleAnimation : ViewAnimation
	{
		[SerializeField]
		float multiplier = 1f;
		[SerializeField]
		AnimationCurve scale = AnimationCurveExtensions.Constant(1f);

		Vector3 GetScale(IView view)
		{
			var dir = (view.transform.position - App.Callbacks.LastCameraOrientation.Position).normalized;
			var dot = Vector3.Dot (dir, App.Callbacks.LastCameraOrientation.Forward);

			return Vector3.one * scale.Evaluate(dot) * multiplier;
		}
		public override void OnShowing(IView view, float scalar)
		{
			view.transform.localScale = GetScale(view);
		}

		public override void OnIdle (IView view)
		{
			view.transform.localScale = GetScale(view);
		}

		public override void OnClosing(IView view, float scalar)
		{
			view.transform.localScale = GetScale(view); 
		}
	}
}