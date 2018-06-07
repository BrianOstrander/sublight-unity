using UnityEngine;

using LunraGames;

namespace LunraGames.SpaceFarm
{
	public interface ILineTransformView : IView
	{
		Vector3 BeginPosition { get; }
		Vector3 EndPosition { get; }
	}

	public class LineTransformAnimation : ViewAnimation
	{
		[SerializeField]
		AnimationCurve showingDistance = AnimationCurveExtensions.Constant(1f);
		[SerializeField]
		AnimationCurve closingDistance = AnimationCurveExtensions.Constant(1f);

		public override void OnShowing(IView view, float scalar)
		{
			OnAnimate(view, scalar, showingDistance);
		}

		public override void OnClosing(IView view, float scalar)
		{
			OnAnimate(view, scalar, closingDistance);
		}

		void OnAnimate(IView view, float scalar, AnimationCurve curve)
		{
			var lineView = view as ILineTransformView;
			if (lineView == null)
			{
				Debug.LogError(view + " is  not an ILineTransformView");
				return;
			}

			var delta = lineView.EndPosition - lineView.BeginPosition;
			lineView.transform.position = lineView.BeginPosition + (delta * curve.Evaluate(scalar));
		}
	}
}