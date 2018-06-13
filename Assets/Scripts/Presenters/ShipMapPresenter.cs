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

			App.Callbacks.StateChange += OnStateChange;
			App.Callbacks.DayTimeDelta += OnDayTimeDelta;
			App.Callbacks.TravelProgress += OnTravelProgress;
			model.Ship.Value.Position.Changed += OnShipPosition;
			model.Ship.Value.Speed.Changed += OnSpeed;
			model.Ship.Value.Rations.Changed += OnRations;
			model.Ship.Value.RationConsumption.Changed += OnRationConsumption;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.StateChange -= OnStateChange;
			App.Callbacks.DayTimeDelta -= OnDayTimeDelta;
			App.Callbacks.TravelProgress -= OnTravelProgress;
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
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End) CloseView(true);
		}

		void OnDayTimeDelta(DayTimeDelta delta)
		{
			var rationsConsumed = model.Ship.Value.Rations.Value - (delta.Delta.DayNormal * model.Ship.Value.RationConsumption);
			model.Ship.Value.Rations.Value = Mathf.Max(0f, rationsConsumed);

			var lastTravel = App.Callbacks.LastTravelProgress;
			if (lastTravel.State == TravelProgress.States.Active)
			{
				// We're traveling!
				var total = lastTravel.Duration.TotalTime;
				var elapsed = DayTime.DayTimeElapsed(lastTravel.StartTime, delta.Current).TotalTime;
				var progress = Mathf.Min(1f, elapsed / total);
				var distance = UniversePosition.Distance(lastTravel.Destination.Position.Value, lastTravel.Origin.Position.Value);
				var normal = (lastTravel.Destination.Position.Value - lastTravel.Origin.Position.Value).Normalized;

				var doneTraveling = Mathf.Approximately(1f, progress);

				var newPos = doneTraveling ? lastTravel.Destination.Position.Value : lastTravel.Origin.Position.Value + new UniversePosition(progress * distance * normal);

				var travel = new TravelProgress(
					doneTraveling ? TravelProgress.States.Complete : TravelProgress.States.Active,
					newPos,
					lastTravel.Origin,
					lastTravel.Destination,
					lastTravel.StartTime,
					lastTravel.EndTime,
					progress
				);
				App.Callbacks.TravelProgress(travel);
			}
		}

		void OnTravelProgress(TravelProgress travelProgress)
		{
			switch (travelProgress.State)
			{
				case TravelProgress.States.Request:
					// TODO: Validation? Eh...
					ship.LastSystem.Value = travelProgress.Origin;
					ship.NextSystem.Value = travelProgress.Destination;
					ship.CurrentSystem.Value = null;
					ship.Position.Value = travelProgress.Origin.Position;
					App.Callbacks.TravelProgress(travelProgress.Duplicate(TravelProgress.States.Active));
					break;
				case TravelProgress.States.Complete:
					ship.LastSystem.Value = travelProgress.Origin;
					ship.NextSystem.Value = null;
					ship.CurrentSystem.Value = travelProgress.Destination;
					ship.Position.Value = travelProgress.Position;
					break;
				case TravelProgress.States.Active:
					ship.Position.Value = travelProgress.Position;
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