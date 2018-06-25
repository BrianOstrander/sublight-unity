using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct CameraOrientation
	{
		public readonly Vector3 Position;
		public readonly Quaternion Rotation;
		public readonly Vector3 Forward;

		public CameraOrientation(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
			Forward = rotation * Vector3.forward;
		}
	}
}