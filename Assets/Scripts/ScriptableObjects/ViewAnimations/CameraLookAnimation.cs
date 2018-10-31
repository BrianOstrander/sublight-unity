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
			if (Camera.main == null) return -view.transform.forward;
			var dir = (view.transform.position - Camera.main.transform.forward).normalized;

			return view.transform.position + (horizontalOnly ? dir.NewY(0).normalized : dir);
		}

		public override void OnShowing(IView view, float scalar)
		{
			view.transform.LookAt(LookAtPosition(view));
		}

		public override void OnLateIdle(IView view)
		{
			if (idleLook) view.transform.LookAt(LookAtPosition(view));
		}

		public override void OnClosing(IView view, float scalar)
		{
			view.transform.LookAt(LookAtPosition(view));
		}
	}
}