using LunraGames.SpaceFarm.Models;
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

			App.Callbacks.StateChange += OnStateChange;
			App.Callbacks.SystemHighlight += OnSystemHighlight;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.StateChange -= OnStateChange;
			App.Callbacks.SystemHighlight -= OnSystemHighlight;
		}

		public void Show(SystemModel model)
		{
			if (View.Visible) return;
			View.Reset();
			View.Closed += OnClose;
			View.Name = model.Seed.Value.ToString();
			ShowView(gameModel.GameplayCanvas, true);
		}

		#region Events
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End)
			{
				nextHighlight = SystemHighlight.None;
				CloseView(true);
			}
		}

		void OnSystemHighlight(SystemHighlight highlight)
		{
			App.Log(highlight.State + " : " + highlight.System.Seed.Value);
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
			//var previousHighlight = nextHighlight;
			//nextHighlight = highlight;
			//if (highlight == previousHighlight) { App.Log("highlights same"); return; }
			//if (highlight.System == previousHighlight.System)
			//{
			//	switch(highlight.State)
			//	{
			//		case SystemHighlight.States.End:
			//		case SystemHighlight.States.Change:
			//			if (View.TransitionState == TransitionStates.Shown) CloseView(true);
			//			break;
			//		case SystemHighlight.States.Begin:
			//			Show(highlight.System);
			//			break;
			//	}


			//	App.Log("lol here");
			//	return;
			//}
			//if (View.TransitionState == TransitionStates.Shown) CloseView(true);
			//else Show(highlight.System);

		}

		void OnClose()
		{
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