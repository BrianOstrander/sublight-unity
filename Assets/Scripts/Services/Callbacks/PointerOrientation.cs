using UnityEngine;

namespace LunraGames.SubLight
{
	public struct PointerOrientation
	{
		/// <summary>
		/// The camera's position in space.
		/// </summary>
		public readonly Vector3 Position;
		public readonly Quaternion Rotation;
		public readonly Vector3 Forward;
		public readonly Vector2 ScreenPosition;

		public PointerOrientation(Vector3 position, Quaternion rotation, Vector2 screenPosition)
		{
			Position = position;
			Rotation = rotation;
			Forward = rotation * Vector3.forward;
			ScreenPosition = screenPosition;
		}
	}
}