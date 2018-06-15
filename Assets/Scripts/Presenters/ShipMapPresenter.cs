using System;

using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipMapPresenter : Presenter<IShipMapView>
	{
		GameModel model;
		ShipModel ship;

		public ShipMapPresenter(GameModel model)
		{
			this.model = model;
			ship = model.Ship;

			App.Callbacks.DayTimeDelta += OnDayTimeDelta;
			App.Callbacks.TravelRequest += OnTravelRequest;
			model.Ship.Value.Position.Changed += OnShipPosition;
			model.Ship.Value.Speed.Changed += OnSpeed;
			model.Ship.Value.Rations.Changed += OnRations;
			model.Ship.Value.RationConsumption.Changed += OnRationConsumption;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.DayTimeDelta -= OnDayTimeDelta;
			App.Callbacks.TravelRequest -= OnTravelRequest;
			model.Ship.Value.Position.Changed -= OnShipPosition;
			model.Ship.Value.Speed.Changed -= OnSpeed;
			model.Ship.Value.Rations.Changed -= OnRations;
			model.Ship.Value.RationConsumption.Changed -= OnRationConsumption;
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
		void OnDayTimeDelta(DayTimeDelta delta)
		{
			var rationsConsumed = model.Ship.Value.Rations.Value - (delta.Delta.DayNormal * model.Ship.Value.RationConsumption);
			model.Ship.Value.Rations.Value = Mathf.Max(0f, rationsConsumed);

			var lastTravel = App.Callbacks.LastTravelRequest;
			if (lastTravel.State == TravelRequest.States.Active)
			{
				// We're traveling!
				var total = lastTravel.Duration.TotalTime;
				var elapsed = DayTime.DayTimeElapsed(lastTravel.StartTime, delta.Current).TotalTime;
				var progress = Mathf.Min(1f, elapsed / total);
				var distance = UniversePosition.Distance(lastTravel.Destination.Position.Value, lastTravel.Origin.Position.Value);

				var normal = (lastTravel.Destination.Position.Value - lastTravel.Origin.Position.Value).Normalized;

				var doneTraveling = Mathf.Approximately(1f, progress);

				var newPos = doneTraveling ? lastTravel.Destination.Position.Value : lastTravel.Origin.Position.Value + new UniversePosition(progress * distance * normal);

				var travel = new TravelRequest(
					doneTraveling ? TravelRequest.States.Complete : TravelRequest.States.Active,
					newPos,
					lastTravel.Origin,
					lastTravel.Destination,
					lastTravel.StartTime,
					lastTravel.EndTime,
					progress
				);
				App.Callbacks.TravelRequest(travel);
			}
		}

		void OnTravelRequest(TravelRequest travelRequest)
		{
			switch (travelRequest.State)
			{
				case TravelRequest.States.Request:
					// TODO: Validation? Eh...
					ship.LastSystem.Value = travelRequest.Origin;
					ship.NextSystem.Value = travelRequest.Destination;
					ship.CurrentSystem.Value = null;
					ship.Position.Value = travelRequest.Origin.Position;
					App.Callbacks.TravelRequest(travelRequest.Duplicate(TravelRequest.States.Active));
					break;
				case TravelRequest.States.Complete:
					ship.LastSystem.Value = null;
					ship.NextSystem.Value = null;
					ship.CurrentSystem.Value = travelRequest.Destination;
					ship.Position.Value = travelRequest.Position;
					App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);
					break;
				case TravelRequest.States.Active:
					ship.Position.Value = travelRequest.Position;
					break;
			}
		}

		void OnShipPosition(UniversePosition position)
		{
			View.UniversePosition = position;
			OnUpdateTravelRadius();
		}

		void OnSpeed(float speed) { OnUpdateTravelRadius(); }

		void OnRations(float rations) { OnUpdateTravelRadius(); }

		void OnRationConsumption(float rationConsumption) { OnUpdateTravelRadius(); }

		void OnUpdateTravelRadius()
		{
			var change = new TravelRadiusChange(ship.Position, ship.Speed, ship.RationConsumption, ship.Rations);
			ship.TravelRadius.Value = change.TravelRadius;
			App.Callbacks.TravelRadiusChange(change);
		}
		#endregion
	}
}