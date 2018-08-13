using System;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class ShipRadiusPresenter : Presenter<IShipRadiusView>, IPresenterCloseShow
	{
		GameModel model;

		public ShipRadiusPresenter(GameModel model)
		{
			this.model = model;

			model.Ship.Value.TravelRadius.Changed += OnTravelRadius;
		}

		protected override void OnUnBind()
		{
			model.Ship.Value.TravelRadius.Changed -= OnTravelRadius;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.UniversePosition = model.Ship.Value.Position;
			View.TravelRadius = model.Ship.Value.TravelRadius;

			ShowView(instant: true);
		}

		public void Close()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView();
		}

		#region Events
		void OnTravelRadius(TravelRadius travelRadius)
		{
			View.UniversePosition = model.Ship.Value.Position;
			View.TravelRadius = travelRadius;
		}
		#endregion
	}
}