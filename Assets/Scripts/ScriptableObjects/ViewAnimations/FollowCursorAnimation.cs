using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public class FollowCursorAnimation : ViewAnimation
	{
		[SerializeField]
		float distance;
		[SerializeField]
		bool idleFollow;
		[SerializeField]
		float smoothingScalar;
		[SerializeField]
		AnimationCurve distanceSmoothing;

		void Follow(IView view)
		{
			// TODO: Make this work without camera main... I guess... since GVRF doesn't support it :/
			/*
			var lastDelta = Quaternion.LookRotation((view.transform.position - Camera.main.transform.position).normalized);
			var currDelta = Quaternion.LookRotation((Camera.main.ScreenToWorldPoint(new Vector3(App.Callbacks.LastPointerOrientation.ScreenPosition.x, App.Callbacks.LastPointerOrientation.ScreenPosition.y, distance)) - Camera.main.transform.position).normalized);

			var angleScalar = Mathf.Clamp01(Quaternion.Dot(lastDelta, currDelta));

			view.transform.position = Camera.main.transform.position + (Quaternion.Slerp(lastDelta, currDelta, distanceSmoothing.Evaluate(angleScalar) * smoothingScalar) * Vector3.forward * distance);
			*/
			var orientation = App.Callbacks.LastPointerOrientation;
			view.transform.position = orientation.Position + (orientation.Rotation * Vector3.forward * distance);
		}

		public override void OnShowing(IView view, float scalar)
		{
			Follow(view);
		}

		public override void OnLateIdle(IView view)
		{
			if (idleFollow) Follow(view);
		}

		public override void OnClosing(IView view, float scalar)
		{
			Follow(view);
		}
	}
}