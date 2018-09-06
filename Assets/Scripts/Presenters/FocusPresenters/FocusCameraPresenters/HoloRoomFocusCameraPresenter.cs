using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class HoloRoomFocusCameraPresenter : FocusCameraPresenter<IHoloRoomFocusCameraView, RoomFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.Room; } }
		protected override bool IsGatherable { get { return false; } }

		public Transform GantryAnchor { get { return View.GantryAnchor; } }

		public HoloRoomFocusCameraPresenter() : base(null) {}

		#region Events
		protected override void OnShowInstant()
		{
			base.OnShowInstant();

			View.Orbit = 0f;
			View.Zoom = 1f;
		}
		#endregion
	}
}