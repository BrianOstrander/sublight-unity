using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class CameraShipView : View, ICameraShipView
	{
		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.forward);
			Gizmos.DrawWireCube(transform.position, Vector3.one);
		}
	}

	public interface ICameraShipView : IView { }
}