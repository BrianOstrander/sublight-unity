using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class HoloRoomFocusCameraPresenter : FocusCameraPresenter<IHoloRoomFocusCameraView, RoomFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.Room; } }
		protected override bool IsGatherable { get { return false; } }

		public Transform GantryAnchor { get { return View.GantryAnchor; } }
		public float FieldOfView { get { return View.FieldOfView; } }

		public HoloRoomFocusCameraPresenter() : base(null) {}

		#region Events
		protected override void OnUpdateEnabled()
		{
			base.OnUpdateEnabled();

			View.Orbit = 0f;
			View.Zoom = 0f;
		}
		#endregion
	}
}