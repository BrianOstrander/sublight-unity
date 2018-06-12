using System;

using UnityEngine;

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
			App.Callbacks.DayTimeDelta += OnDayTimeDelta;
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
		}

		void OnShipPosition(UniversePosition position)
		{
			View.UniversePosition = position;
		}

		void OnSpeed(float speed) { OnUpdateTravelRadius(); }

		void OnRations(float rations) { OnUpdateTravelRadius(); }

		void OnRationConsumption(float rationConsumption) { OnUpdateTravelRadius(); }

		void OnUpdateTravelRadius()
		{
			var rationDistance = (model.Ship.Value.Rations / model.Ship.Value.RationConsumption) * model.Ship.Value.Speed;
			model.Ship.Value.TravelRadius.Value = new TravelRadius(rationDistance * 0.8f, rationDistance * 0.9f, rationDistance);
		}
		#endregion
	}
}