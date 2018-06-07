using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct CameraOrientation
	{
		public Vector3 Position { get; private set; }
		public Quaternion Rotation { get; private set; }
		public Vector3 Forward { get; private set; }

		public CameraOrientation(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
			Forward = rotation * Vector3.forward;
		}
	}
}