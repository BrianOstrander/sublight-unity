﻿using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SystemDetailPresenter : Presenter<ISystemDetailView>
	{
		GameModel gameModel;

		SystemHighlight nextHighlight;

		public SystemDetailPresenter(GameModel gameModel)
		{
			this.gameModel = gameModel;

			App.Callbacks.SystemHighlight += OnSystemHighlight;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.SystemHighlight -= OnSystemHighlight;
		}

		public void Show(SystemModel model)
		{
			if (View.Visible) return;
			View.Reset();
			View.Closed += OnClosed;
			View.Name = model.Name;
			View.DayTravelTime = UniversePosition.TravelTime(model.Position, gameModel.Ship.Value.Position, gameModel.Ship.Value.Speed).DayCeiling;

			ShowView(gameModel.GameplayCanvas, true);
		}

		#region Events
		void OnSystemHighlight(SystemHighlight highlight)
		{
			nextHighlight = highlight;
			switch (highlight.State)
			{
				case SystemHighlight.States.End:
				case SystemHighlight.States.Change:
					if (View.TransitionState == TransitionStates.Shown) CloseView(true);
					break;
				case SystemHighlight.States.Begin:
					Show(highlight.System);
					break;
			}
		}

		void OnClosed()
		{
			if (App.SM.CurrentEvent == StateMachine.Events.End) return;

			switch (nextHighlight.State)
			{
				case SystemHighlight.States.Begin:
				case SystemHighlight.States.Change:
					Show(nextHighlight.System);
					break;
			}
		}
		#endregion
	}
}