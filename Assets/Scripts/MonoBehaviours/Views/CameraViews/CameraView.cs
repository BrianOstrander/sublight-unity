using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class CameraView : View, ICameraView
	{
		public Vector3 Position  { set { transform.localPosition = value; } }
		public Quaternion Rotation { set { transform.localRotation = value; } }

		public override void Reset()
		{
			base.Reset();

			Position = Vector3.zero;
			Rotation = Quaternion.identity;
		}
	}
}