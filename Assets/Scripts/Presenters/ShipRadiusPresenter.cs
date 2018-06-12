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

			App.Callbacks.StateChange += OnStateChange;
			App.Callbacks.TravelRadiusChange += OnTravelRadiusChange;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.StateChange -= OnStateChange;
			App.Callbacks.TravelRadiusChange -= OnTravelRadiusChange;
		}

		public void Show(Action done = null)
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = model.Ship.Value.Position;
			View.TravelRadius = model.Ship.Value.TravelRadius;
			if (done != null) View.Shown += done;
			ShowView(instant: true);
		}

		#region Events
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End) CloseView(true);
		}

		void OnTravelRadiusChange(TravelRadiusChange travelRadiusChange)
		{
			View.UniversePosition = travelRadiusChange.Origin;
			View.TravelRadius = travelRadiusChange.TravelRadius;
		}
		#endregion
	}
}