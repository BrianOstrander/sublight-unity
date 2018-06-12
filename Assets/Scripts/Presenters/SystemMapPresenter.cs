using System;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SystemMapPresenter : Presenter<ISystemMapView>
	{
		GameModel gameModel;
		SystemModel model;

		bool isTravelable;

		public SystemMapPresenter(GameModel gameModel, SystemModel model)
		{
			this.gameModel = gameModel;
			this.model = model;
			SetView(App.V.Get<ISystemMapView>(v => v.SystemType == model.SystemType));

			gameModel.Ship.Value.Position.Changed += OnShipPosition;
			gameModel.Ship.Value.TravelRadius.Changed += OnTravelRadius;
			App.Callbacks.StateChange += OnStateChange;
		}

		protected override void UnBind()
		{
			base.UnBind();

			gameModel.Ship.Value.Position.Changed -= OnShipPosition;
			gameModel.Ship.Value.TravelRadius.Changed -= OnTravelRadius;
			App.Callbacks.StateChange -= OnStateChange;
		}

		public void Show(Action done = null)
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = model.Position;
			View.Highlight = OnHighlight;
			View.Click = OnClick;
			OnCheckTravelable();
			if (done != null) View.Shown += done;
			ShowView(instant: true);
		}

		#region Events
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End) CloseView(true);
		}

		void OnHighlight(bool highlighted)
		{
			var state = SystemHighlight.States.End;
			if (highlighted)
			{
				switch (App.Callbacks.LastSystemHighlight.State)
				{
					case SystemHighlight.States.Unknown:
					case SystemHighlight.States.End:
						state = SystemHighlight.States.Begin;
						break;
					case SystemHighlight.States.Begin:
					case SystemHighlight.States.Change:
						state = SystemHighlight.States.Change;
						break;
				}
			}
			else
			{
				switch (App.Callbacks.LastSystemHighlight.State)
				{
					case SystemHighlight.States.Change:
						if (App.Callbacks.LastSystemHighlight.System != model) return;
						break;
				}
			}
			App.Callbacks.SystemHighlight(new SystemHighlight(state, model));
		}

		void OnClick()
		{
			if (!isTravelable)
			{
				App.Log("Too far to travel here");
			}
		}

		void OnShipPosition(UniversePosition position) { OnCheckTravelable(); }

		void OnTravelRadius(TravelRadius travelRadius) { OnCheckTravelable(); }

		void OnCheckTravelable()
		{
			var distance = App.UniverseService.UniverseDistance(model.Position, gameModel.Ship.Value.Position);
			isTravelable = distance < gameModel.Ship.Value.TravelRadius.Value.MaximumRadius;
		}
		#endregion
	}
}