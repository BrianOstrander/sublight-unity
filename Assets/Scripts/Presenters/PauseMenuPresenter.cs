using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
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
			if (App.Callbacks.LastPlayState.State != PlayState.States.Paused) App.Callbacks.PlayState(PlayState.Paused);

			if (cacheSpeed) lastSpeedChange = App.Callbacks.LastSpeedRequest;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);

			View.Reset();

			View.BackClick = OnBackClick;
			View.SaveClick = OnSaveClick;
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
			View.Closed += () => App.Callbacks.PlayState(PlayState.Playing);
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

		void OnSaveClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			App.Callbacks.PopEscape();
			View.Interactable = false;
			App.Callbacks.SaveRequest(SaveRequest.Request(OnSaveRequest));
		}

		void OnSaveRequest(SaveRequest request)
		{
			switch(request.State)
			{
				case SaveRequest.States.Complete:
					if (request.Status == RequestStatus.Success)
					{
						OnBackClick();
						break;
					}
					CloseView();
					App.Callbacks.DialogRequest(
						DialogRequest.Alert(
							LanguageStringModel.Override(request.Error),
							DialogStyles.Neutral,
							LanguageStringModel.Override("Cannot Save"),
							OnSaveFailAlert
						)
					);
					break;
			}
		}

		void OnSaveFailAlert()
		{
			ShowFromDialog();
		}

		void OnMainMenuClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			App.Callbacks.PopEscape();
			CloseView();
			App.Callbacks.DialogRequest(
				DialogRequest.CancelConfirm(
					LanguageStringModel.Override(Strings.ConfirmToMainMenu),
					done: OnMainMenuConfirm
				)
			);
		}

		void OnMainMenuConfirm(RequestStatus status)
		{
			switch (status)
			{
				case RequestStatus.Success:
					if (App.Callbacks.LastPlayState.State != PlayState.States.Playing) App.Callbacks.PlayState(PlayState.Playing);
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