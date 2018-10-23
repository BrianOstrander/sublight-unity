using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridPresenter : FocusPresenter<IGridView, SystemFocusDetails>
	{
		GameModel model;

		public GridPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			App.Callbacks.CurrentScrollGesture += OnCurrentScrollGesture;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Callbacks.CurrentScrollGesture -= OnCurrentScrollGesture;
		}

		protected override void OnUpdateEnabled()
		{
			model.Zoom.Value = View.UpdateZoom(model.Zoom);
		}

		#region
		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnCurrentScrollGesture(ScrollGesture gesture)
		{
			if (!View.Visible || !View.Highlighted) return;

			model.Zoom.Value = View.UpdateZoom(model.Zoom, gesture.Current.y * gesture.TimeDelta);
		}

		#endregion
	}
}