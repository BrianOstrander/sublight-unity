using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class CameraEncounterView : View, ICameraEncounterView
	{
		void OnDrawGizmos()
		{
			if (!Application.isPlaying) return;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.forward);
			Gizmos.DrawWireCube(transform.position, Vector3.one);
		}
	}

	public interface ICameraEncounterView : IView {}
}