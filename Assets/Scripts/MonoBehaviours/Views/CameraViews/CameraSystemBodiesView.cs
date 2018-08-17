using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class CameraSystemBodiesView : View, ICameraSystemBodiesView
	{
		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.forward);
			Gizmos.DrawWireCube(transform.position, Vector3.one);
		}
	}

	public interface ICameraSystemBodiesView : IView {}
}