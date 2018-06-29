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

			model.Ship.Value.TravelRadius.Changed += OnTravelRadius;
		}

		protected override void UnBind()
		{
			base.UnBind();

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

		#region Events
		void OnTravelRadius(TravelRadius travelRadius)
		{
			View.UniversePosition = model.Ship.Value.Position;
			View.TravelRadius = travelRadius;
		}
		#endregion
	}
}