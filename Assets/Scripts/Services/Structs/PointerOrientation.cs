using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct PointerOrientation
	{
		public Vector3 Position { get; private set; }
		public Quaternion Rotation { get; private set; }
		public Vector3 Forward { get; private set; }
		public Vector2 ScreenPosition { get; private set; }

		public PointerOrientation(Vector3 position, Quaternion rotation, Vector2 screenPosition)
		{
			Position = position;
			Rotation = rotation;
			Forward = rotation * Vector3.forward;
			ScreenPosition = screenPosition;
		}
	}
}