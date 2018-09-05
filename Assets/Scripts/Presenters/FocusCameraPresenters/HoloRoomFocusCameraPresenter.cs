using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class HoloRoomFocusCameraPresenter : FocusCameraPresenter<IHoloRoomFocusCameraView, RoomFocusDetails>
	{
		protected override SetFocusLayers FocusLayer { get { return SetFocusLayers.Room; } }

		#region Events
		protected override void OnShowInstant()
		{
			View.Orbit = 0f;
			View.Zoom = 1f;
		}
		#endregion
	}
}