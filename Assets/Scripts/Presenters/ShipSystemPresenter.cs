using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipSystemPresenter : Presenter<IShipSystemView>, IPresenterCloseShow
	{
		GameModel model;
		ShipModel ship;

		public ShipSystemPresenter(GameModel model)
		{
			this.model = model;
			ship = model.Ship;

			App.Callbacks.DayTimeDelta += OnDayTimeDelta;
			model.TravelRequest.Changed += OnTravelRequest;
			model.Ship.Value.Position.Changed += OnShipPosition;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.DayTimeDelta -= OnDayTimeDelta;
			model.TravelRequest.Changed -= OnTravelRequest;
			model.Ship.Value.Position.Changed -= OnShipPosition;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.UniversePosition = model.Ship.Value.Position;

			ShowView(instant: true);
		}

		public void Close()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView();
		}

		#region Events
		void OnDayTimeDelta(DayTimeDelta delta)
		{
			var rationsConsumed = model.Ship.Value.Inventory.Resources.Rations.Value - (delta.Delta.TotalTime * model.Ship.Value.RationConsumption);
			model.Ship.Value.Inventory.Resources.Rations.Value = Mathf.Max(0f, rationsConsumed);

			var lastTravel = model.TravelRequest.Value;
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
					lastTravel.FuelConsumed,
					progress
				);
				model.TravelRequest.Value = travel;
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
					model.TravelRequest.Value = travelRequest.Duplicate(TravelRequest.States.Active);
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
			if (View.TransitionState == TransitionStates.Shown) View.UniversePosition = position;
		}
		#endregion
	}
}