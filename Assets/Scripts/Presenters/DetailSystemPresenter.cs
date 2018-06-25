using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;
using UnityEngine;

namespace LunraGames.SpaceFarm.Presenters
{
	public class DetailSystemPresenter : Presenter<IDetailSystemView>
	{
		GameModel gameModel;

		SystemHighlight nextHighlight;

		public DetailSystemPresenter(GameModel gameModel)
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
			View.DayTravelTime = Mathf.Min(1, UniversePosition.TravelTime(model.Position, gameModel.Ship.Value.Position, gameModel.Ship.Value.Speed).Day);

			ShowView(App.GameCanvasRoot, true);
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