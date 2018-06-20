using System;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipRadiusPresenter : Presenter<IShipRadiusView>
	{
		GameModel model;

		public ShipRadiusPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.TravelRadiusChange += OnTravelRadiusChange;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.TravelRadiusChange -= OnTravelRadiusChange;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.UniversePosition = model.Ship.Value.Position;
			View.TravelRadius = model.Ship.Value.TravelRadius;

			ShowView(instant: true);
		}

		#region Events
		void OnTravelRadiusChange(TravelRadiusChange travelRadiusChange)
		{
			View.UniversePosition = travelRadiusChange.Origin;
			View.TravelRadius = travelRadiusChange.TravelRadius;
		}
		#endregion
	}
}