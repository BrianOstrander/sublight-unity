using System;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipCameraPresenter : Presenter<IShipCameraView>
	{
		public void Show(Action done)
		{
			if (View.Visible) return;
			View.Reset();
			View.Shown += done;
			ShowView(instant: true);
		}
	}
}