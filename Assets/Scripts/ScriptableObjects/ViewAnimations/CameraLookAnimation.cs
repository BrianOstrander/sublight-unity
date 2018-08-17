using UnityEngine;
using LunraGames;

namespace LunraGames.SubLight
{
	public class CameraLookAnimation : ViewAnimation
	{
		[SerializeField]
		bool horizontalOnly;
		[SerializeField]
		bool idleLook;

		Vector3 LookAtPosition(IView view)
		{
			var dir = (view.transform.position - App.Callbacks.LastCameraOrientation.Position).normalized;

			return view.transform.position + (horizontalOnly ? dir.NewY(0).normalized : dir);
		}

		public override void OnShowing(IView view, float scalar)
		{
			view.transform.LookAt(LookAtPosition(view));
		}

		public override void OnIdle(IView view)
		{
			if (idleLook) view.transform.LookAt(LookAtPosition(view));
		}

		public override void OnClosing(IView view, float scalar)
		{
			view.transform.LookAt(LookAtPosition(view));
		}
	}
}