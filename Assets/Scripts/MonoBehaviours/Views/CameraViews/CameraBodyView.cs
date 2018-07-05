using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class CameraBodyView : View, ICameraBodyView
	{
		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.forward);
			Gizmos.DrawWireCube(transform.position, Vector3.one);
		}
	}

	public interface ICameraBodyView : IView { }
}