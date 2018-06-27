using System;

using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipSystemPresenter : Presenter<IShipSystemView>
	{
		GameModel model;
		ShipModel ship;

		public ShipSystemPresenter(GameModel model)
		{
			this.model = model;
			ship = model.Ship;

			App.Callbacks.DayTimeDelta += OnDayTimeDelta;
			App.Callbacks.TravelRequest += OnTravelRequest;
			model.Ship.Value.Position.Changed += OnShipPosition;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.DayTimeDelta -= OnDayTimeDelta;
			App.Callbacks.TravelRequest -= OnTravelRequest;
			model.Ship.Value.Position.Changed -= OnShipPosition;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.UniversePosition = model.Ship.Value.Position;

			ShowView(instant: true);
		}

		#region Events
		void OnDayTimeDelta(DayTimeDelta delta)
		{
			var rationsConsumed = model.Ship.Value.Rations.Value - (delta.Delta.TotalDays * model.Ship.Value.RationConsumption);
			model.Ship.Value.Rations.Value = Mathf.Max(0f, rationsConsumed);

			var lastTravel = App.Callbacks.LastTravelRequest;
			if (lastTravel.State == TravelRequest.States.Active)
			{
				// We're traveling!
				var progress = lastTravel.GetProgress(delta.Current);
				var distance = UniversePosition.Distance(lastTravel.Destination, lastTravel.Origin);

				var normal = (lastTravel.Destination - lastTravel.Origin).Normalized;

				var doneTraveling = Mathf.Approximately(1f, progress);

				var newPos = doneTraveling ? lastTravel.Destination : lastTravel.Origin + new UniversePosition(progress * distance * normal);

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
					ship.CurrentSystem.Value = UniversePosition.Zero;
					ship.Position.Value = travelRequest.Origin;
					if (Mathf.Approximately(App.Callbacks.LastSpeedRequest.Speed, 0f)) App.Callbacks.SpeedRequest(SpeedRequest.PlayRequest);
					App.Callbacks.TravelRequest(travelRequest.Duplicate(TravelRequest.States.Active));
					break;
				case TravelRequest.States.Complete:
					ship.LastSystem.Value = UniversePosition.Zero;
					ship.NextSystem.Value = UniversePosition.Zero;
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
		}
		#endregion
	}
}