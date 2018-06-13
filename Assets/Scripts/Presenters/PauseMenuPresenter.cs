using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class PauseMenuPresenter : Presenter<IPauseMenuView>
	{
		GameModel model;

		public PauseMenuPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.Escape += OnEscape;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.Escape -= OnEscape;
		}

		void Show()
		{
			if (View.Visible) return;
			View.Reset();
			View.BackClick = OnBackClick;
			View.MainMenuClick = OnMainMenuClick;
			ShowView(model.GameplayCanvas, true);
		}

		#region Events
		void OnEscape()
		{
			switch(View.TransitionState)
			{
				case TransitionStates.Closed:
					ShowView();
					break;
				case TransitionStates.Shown:
					CloseView(true);
					break;
			}
		}

		void OnBackClick()
		{
			CloseView(true);
		}

		void OnMainMenuClick()
		{
			var payload = new HomePayload();
			App.SM.RequestState(payload);
		}
		#endregion

	}
}