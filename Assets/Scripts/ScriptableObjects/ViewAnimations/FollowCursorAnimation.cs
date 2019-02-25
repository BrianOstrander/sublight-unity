using UnityEngine;

namespace LunraGames.SubLight
{
	public class FollowCursorAnimation : ViewAnimation
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float distance;
		[SerializeField]
		bool idleFollow;
		[SerializeField]
		float smoothingScalar;
		[SerializeField]
		AnimationCurve distanceSmoothing;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		void Follow(IView view)
		{
			var orientation = App.Callbacks.LastPointerOrientation;
			view.transform.position = orientation.Position + (orientation.Rotation * Vector3.forward * distance);
		}

		public override void OnShowing(IView view, float scalar)
		{
			Follow(view);
		}

		public override void OnLateIdle(IView view, float delta)
		{
			if (idleFollow) Follow(view);
		}

		public override void OnClosing(IView view, float scalar)
		{
			Follow(view);
		}
	}
}