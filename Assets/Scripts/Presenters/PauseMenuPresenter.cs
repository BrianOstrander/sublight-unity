using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class PauseMenuPresenter : Presenter<IPauseMenuView>
	{
		SpeedRequest lastSpeedChange;

		public PauseMenuPresenter(bool pushEscapable = true)
		{
			if (pushEscapable) App.Callbacks.PushEscape(new EscapeEntry(ShowFromGame, true, true));
		}

		void Show(bool cacheSpeed = true)
		{
			if (View.Visible) return;

			if (cacheSpeed) lastSpeedChange = App.Callbacks.LastSpeedRequest;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);

			View.Reset();
			View.BackClick = OnBackClick;
			View.MainMenuClick = OnMainMenuClick;
			ShowView(App.OverlayCanvasRoot);
		}

		#region Events
		void ShowFromGame()
		{
			Show();
			View.Shown += () => App.Callbacks.PushEscape(new EscapeEntry(CloseToGame, false, false));
		}

		void ShowFromDialog()
		{
			Show(false);
			View.Shown += () => App.Callbacks.PushEscape(new EscapeEntry(CloseToGame, false, false));
		}

		void CloseToGame()
		{
			View.Closed += () => App.Callbacks.PushEscape(new EscapeEntry(ShowFromGame, true, true));
			View.Closed += () => App.Callbacks.SpeedRequest(lastSpeedChange.Duplicate(SpeedRequest.States.Request));
			CloseView();
		}

		void OnBackClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			App.Callbacks.PopEscape();
			App.Callbacks.ShadeRequest(ShadeRequest.UnShade);
			App.Callbacks.ObscureCameraRequest(ObscureCameraRequest.UnObscure);
			CloseToGame();
		}

		void OnMainMenuClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			App.Callbacks.PopEscape();
			CloseView();
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
					ShowFromDialog();
					break;
			}
		}
		#endregion

	}
}