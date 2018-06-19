using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class PauseMenuPresenter : Presenter<IPauseMenuView>
	{
		GameModel model;
		SpeedRequest lastSpeedChange;

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
			lastSpeedChange = App.Callbacks.LastSpeedRequest;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);
			View.Closed += OnClosed;
			View.BackClick = OnBackClick;
			View.MainMenuClick = OnMainMenuClick;
			ShowView(model.GameplayCanvas, true);
		}

		#region Events
		void OnEscape()
		{
			switch(View.TransitionState)
			{
				case TransitionStates.Unknown:
				case TransitionStates.Closed:
					Show();
					break;
				case TransitionStates.Shown:
					CloseView(true);
					break;
			}
		}

		void OnBackClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView(true);
		}

		void OnMainMenuClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView(true);
			App.Callbacks.DialogRequest(
				DialogRequest.CancelConfirm(
					Strings.ConfirmToMainMenu,
					done: OnMainMenuConfirm
				)
			);
		}

		void OnMainMenuConfirm(RequestStatus status)
		{
			switch (status)
			{
				case RequestStatus.Success:
					var payload = new HomePayload();
					App.SM.RequestState(payload);
					break;
				default:
					Show();
					break;
			}
		}

		void OnClosed()
		{
			if (App.SM.CurrentEvent == StateMachine.Events.End) return;

			App.Callbacks.SpeedRequest(lastSpeedChange.Duplicate(SpeedRequest.States.Request));
		}
		#endregion

	}
}