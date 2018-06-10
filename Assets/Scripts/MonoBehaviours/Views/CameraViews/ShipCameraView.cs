using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class ShipCameraView : View, IShipCameraView
	{
		public Vector3 Position
		{
			set { transform.localPosition = value; }
			private get { return transform.localPosition; }
		}

		public Quaternion Rotation
		{
			set { transform.localRotation = value; }
			private get { return transform.localRotation; }
		}

		Vector3? dragStart;

		public override void Reset()
		{
			base.Reset();

			Position = Vector3.zero;
			Rotation = Quaternion.identity;
		}

		public void Drag(Vector2 screenDelta, bool isDone = false)
		{
			if (!dragStart.HasValue) dragStart = Position;

		}

		void OnDrawGizmos()
		{
			Gizmos.DrawRay(Position, Rotation * Vector3.forward);
		}
	}

	public interface IShipCameraView : ICameraView
	{
		void Drag(Vector2 screenDelta, bool isDone = false);
	}
}