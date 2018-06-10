using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class ShipCameraView : View, IShipCameraView
	{
		[SerializeField]
		Transform dragRoot;
		[SerializeField]
		Transform dragForward;

		public Transform DragRoot { get { return dragRoot; } }

		public Transform DragForward { get { return dragForward; } }

		public override void Reset()
		{
			base.Reset();
		}
	}

	public interface IShipCameraView : IDragView {}
}