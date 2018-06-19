using UnityEngine;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm
{
	public class RotateDragAnimation : ViewAnimation
	{
		[SerializeField]
		float scalar;

		Gesture lastGesture;

		public override void OnIdle(IView view)
		{
			if (!(view is IDragView) || App.Callbacks.LastObscureCameraRequest.IsObscured) return;
			var dragView = view as IDragView;
			var gesture = App.Callbacks.LastGesture;
			var camera = App.Callbacks.LastCameraOrientation;

			if ((!gesture.IsSecondary) || (lastGesture.IsGesturing == gesture.IsGesturing && !lastGesture.IsGesturing))
			{
				lastGesture = gesture;
				return;
			}

			if (!lastGesture.IsGesturing && gesture.IsGesturing)
			{
				// We're starting
				var plane = new Plane(Vector3.up, dragView.Root.position);
				var dist = 0f;
				plane.Raycast(new Ray(camera.Position, camera.Forward), out dist);
				dragView.DragAxis = camera.Position + (camera.Forward * dist);
				lastGesture = gesture;
			}
			else if (lastGesture.IsGesturing && !gesture.IsGesturing)
			{
				// We're ending

				var newPos = dragView.DragRoot.position;
				dragView.Root.position = newPos;
				dragView.DragRoot.localPosition = Vector3.zero;

				lastGesture = gesture;
				return;
			}

			dragView.DragRoot.RotateAround(dragView.DragAxis, Vector3.up, (gesture.Delta.x - lastGesture.Delta.x) * scalar);

			//gesture.b

			//var planeForward = dragView.DragForward.forward.FlattenY() * gesture.Delta.y * -1f;
			//var planeRight = dragView.DragForward.right.FlattenY() * gesture.Delta.x * -1f;
			//var planeCombined = planeForward + planeRight;

			//dragView.DragRoot.localPosition = planeCombined * scalar;

			lastGesture = gesture;
		}
	}
}