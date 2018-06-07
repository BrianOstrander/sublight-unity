using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public interface ICameraView : IView
	{
		Vector3 Position { set; }
		Quaternion Rotation { set; }
	}
}