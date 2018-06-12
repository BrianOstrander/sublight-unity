using System;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipMapPresenter : Presenter<IShipMapView>
	{
		GameModel model;

		public ShipMapPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.StateChange += OnStateChange;
			model.Ship.Value.Position.Changed += OnShipPosition;
			model.Ship.Value.Speed.Changed += OnSpeed;
			model.Ship.Value.Rations.Changed += OnRations;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.StateChange -= OnStateChange;
			model.Ship.Value.Position.Changed -= OnShipPosition;
			model.Ship.Value.Speed.Changed -= OnSpeed;
			model.Ship.Value.Rations.Changed -= OnRations;
		}

		public void Show(Action done = null)
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = model.Ship.Value.Position;
			OnUpdateTravelRadius();
			if (done != null) View.Shown += done;
			ShowView(instant: true);
		}

		#region Events
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End) CloseView(true);
		}

		//void OnTime()

		void OnShipPosition(UniversePosition position)
		{
			View.UniversePosition = position;
		}

		void OnSpeed(float speed) { OnUpdateTravelRadius(); }

		void OnRations(float rations) { OnUpdateTravelRadius(); }

		void OnUpdateTravelRadius()
		{
			var rationDistance = model.Ship.Value.Rations * model.Ship.Value.Speed;
			model.Ship.Value.TravelRadius.Value = new TravelRadius(rationDistance * 0.8f, rationDistance * 0.9f, rationDistance);
		}
		#endregion
	}
}