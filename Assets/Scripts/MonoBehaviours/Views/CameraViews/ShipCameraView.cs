using UnityEngine;

namespace LunraGames.SpaceFarm.Views
{
	public class ShipCameraView : View, IShipCameraView
	{
		[SerializeField]
		Transform dragRoot;
		[SerializeField]
		Transform dragForward;
		[SerializeField]
		Transform dragAxisRoot;

		public Transform DragRoot { get { return dragRoot; } }
		public Transform DragForward { get { return dragForward; } }
		public Transform DragAxisRoot { get { return dragAxisRoot; } }
		public Vector3 DragAxis { get; set; }

		public override void Reset()
		{
			base.Reset();
		}
	}

	public interface IShipCameraView : IDragView {}
}