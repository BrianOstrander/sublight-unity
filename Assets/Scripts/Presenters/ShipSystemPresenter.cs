using System.Linq;

using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipSystemPresenter : Presenter<IShipSystemView>, IPresenterCloseShow
	{
		GameModel model;
		ShipModel ship;

		string[] lastEstimatedFailures = new string[0];
		string[] lastFailures = new string[0];

		public ShipSystemPresenter(GameModel model)
		{
			this.model = model;
			ship = model.Ship;

			App.Callbacks.DayTimeDelta += OnDayTimeDelta;
			App.Callbacks.ClearInventoryRequest += OnClearInventory;
			model.TravelRequest.Changed += OnTravelRequest;

			ship.Position.Changed += OnShipPosition;
			ship.Inventory.NextEstimatedFailure.Changed += OnNextEstimatedFailure;
			ship.Inventory.NextFailure.Changed += OnNextFailure;
			ship.Inventory.EstimatedFailureIds.Changed += OnModulesEstimatedFailure;
			ship.Inventory.FailureIds.Changed += OnModulesFailure;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.DayTimeDelta -= OnDayTimeDelta;
			App.Callbacks.ClearInventoryRequest -= OnClearInventory;
			model.TravelRequest.Changed -= OnTravelRequest;

			ship.Position.Changed -= OnShipPosition;
			ship.Inventory.NextEstimatedFailure.Changed -= OnNextEstimatedFailure;
			ship.Inventory.NextFailure.Changed -= OnNextFailure;
			ship.Inventory.EstimatedFailureIds.Changed -= OnModulesEstimatedFailure;
			ship.Inventory.FailureIds.Changed -= OnModulesFailure;
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
		void OnClearInventory(ClearInventoryRequest request)
		{
			ship.Inventory.ClearUnused();
		}

		void OnDayTimeDelta(DayTimeDelta delta)
		{
			var refill = ship.Inventory.RefillResources.Duplicate.Multiply(delta.Delta.TotalTime);
			var refillLogic = ship.Inventory.RefillLogisticsResources.Duplicate.Multiply(delta.Delta.TotalTime);

			var newResources = ship.Inventory.AllResources.Duplicate.Add(refill);

			var spaceInLogistics = ship.Inventory.MaximumRefillableLogisticsResources.Duplicate.Subtract(newResources);

			newResources.Add(refillLogic.Clamp(spaceInLogistics)).ClampNegatives();

			ship.Inventory.AllResources.Assign(newResources);

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

			// Module failure
			ship.Inventory.CurrentDayTime.Value = delta.Current;
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
					ship.Inventory.AllResources.Fuel.Value -= travelRequest.FuelConsumed;
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

		void OnNextEstimatedFailure(DayTime nextEstimatedFailure)
		{
			Debug.Log("The next estimated failure is " + nextEstimatedFailure);
		}

		void OnNextFailure(DayTime nextFailure)
		{
			Debug.Log("The next failure is " + nextFailure);
		}

		void OnModulesEstimatedFailure(string[] instanceIds)
		{
			var differences = instanceIds.Except(lastEstimatedFailures).ToArray();
			lastEstimatedFailures = instanceIds;

			if (differences.Length != 0) OnModulesShared("The following modules are going to fail!", "Modules Are Failing!", differences);
		}

		void OnModulesFailure(string[] instanceIds)
		{
			var differences = instanceIds.Except(lastFailures).ToArray();
			lastFailures = instanceIds;

			if (differences.Length != 0) OnModulesShared("The following modules have failed!", "Modules Have Failed!", differences);

		}

		void OnModulesShared(string prefix, string title, string[] instanceIds)
		{
			var result = prefix;
			foreach (var value in instanceIds)
			{
				result += "\n - " + ship.Inventory.GetInventoryFirstOrDefault(value).Name.Value;
			}

			App.Callbacks.DialogRequest(DialogRequest.Alert(result, title));
		}
		#endregion
	}
}