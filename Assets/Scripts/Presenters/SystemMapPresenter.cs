﻿using System;

using UnityEngine;

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
			View.UniversePosition = model.Position;
			View.Highlight = OnHighlight;
			View.Click = OnClick;
			OnTravelRadiusChange(App.Callbacks.LastTravelRadiusChange);
			OnTravelColor();
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
			if (isTravelable)
			{
				
				var travel = new TravelProgress(
					TravelProgress.States.Request,
					gameModel.Ship.Value.CurrentSystem.Value.Position,
					gameModel.Ship.Value.CurrentSystem,
					model,
					App.Callbacks.LastDayTimeDelta.Current,
					UniversePosition.TravelTime(gameModel.Ship.Value.CurrentSystem.Value.Position.Value, model.Position.Value, gameModel.Ship.Value.Speed.Value),
					0f
				);
				App.Callbacks.TravelProgress(travel);
			}
			else
			{
				App.Log("Too far to travel here");
			}
		}

		void OnTravelRadiusChange(TravelRadiusChange travelRadiusChange)
		{
			var distance = UniversePosition.Distance(model.Position, travelRadiusChange.Origin);
			isTravelable = distance < travelRadiusChange.TravelRadius.MaximumRadius;
			if (View.Visible) OnTravelColor();
		}

		void OnTravelColor()
		{
			View.TravelColor = isTravelable ? Color.white : Color.red;
		}
		#endregion
	}
}