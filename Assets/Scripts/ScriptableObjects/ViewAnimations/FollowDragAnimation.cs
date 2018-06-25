using UnityEngine;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm
{
	public class FollowDragAnimation : ViewAnimation
	{
		[SerializeField]
		float scalar;

		Gesture lastGesture;
		 
		public override void OnIdle(IView view)
		{
			if (!(view is IDragView) || App.Callbacks.LastObscureCameraRequest.IsObscured) return;
			var dragView = view as IDragView;
			var gesture = App.Callbacks.LastGesture;

			if ((gesture.IsSecondary) || (lastGesture.IsGesturing == gesture.IsGesturing && !lastGesture.IsGesturing))
			{
				lastGesture = gesture;
				return;
			}

			if (lastGesture.IsGesturing && !gesture.IsGesturing)
			{
				var newPos = dragView.DragRoot.position;
				dragView.Root.position = newPos;
				dragView.DragRoot.localPosition = Vector3.zero;

				lastGesture = gesture;
				return;
			}

			var planeForward = dragView.DragForward.forward.FlattenY() * gesture.Delta.y * -1f;
			var planeRight = dragView.DragForward.right.FlattenY() * gesture.Delta.x * -1f;
			var planeCombined = planeForward + planeRight;

			dragView.DragRoot.localPosition = planeCombined * scalar;

			lastGesture = gesture;
		}
	}
}