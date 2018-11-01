using UnityEngine;

namespace LunraGames.SubLight
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