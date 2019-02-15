using System;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class MainMenuOptionsPresenter : Presenter<IRadialOptionsView>, IPresenterCloseShowOptions
	{
		HomePayload payload;
		MainMenuLanguageBlock language;

		public MainMenuOptionsPresenter(
			HomePayload payload,
			MainMenuLanguageBlock language
		)
		{
			this.payload = payload;
			this.language = language;
		}

		public void Show(Transform parent = null, bool instant = false)
		{
			OnShow(true, parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);
		}

		void OnShow(bool longTransition, Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			View.LongTransition = longTransition;

			View.ButtonsLeft = new LabelButtonBlock[]
			{
				new LabelButtonBlock(language.NewGame, CloseThenClick(OnNewGameClick)),
				new LabelButtonBlock(language.ContinueGame, CloseThenClick(OnContinueGameClick), payload.CanContinueSave)
			};

			View.ButtonsRight = new LabelButtonBlock[]
			{
				new LabelButtonBlock(language.Settings, CloseThenClick(OnSettingsClick)),
				new LabelButtonBlock(language.Credits, CloseThenClick(OnCreditsClick)),
				new LabelButtonBlock(language.Quit, CloseThenClick(OnQuitClick))
			};

			ShowView(parent, instant);
		}

		void PushQuitRequest()
		{
			SM.Push(
				() =>
				{
					var quitPayload = new QuitPayload();
					quitPayload.Requester = "HomeMainMenu";
					App.SM.RequestState(quitPayload);
				},
				"QuittingFromMainMenu"
			);
		}

		#region Click Events
		Action CloseThenClick(Action done)
		{
			return () =>
			{
				if (View.TransitionState != TransitionStates.Shown) return;
				View.Closed += done;
				CloseView();
			};
		}

		void OnNewGameClick()
		{
			if (payload.CanContinueSave)
			{
				App.Callbacks.DialogRequest(
					DialogRequest.ConfirmDeny(
						language.NewGameOverwriteConfirm.Message,
						DialogStyles.Warning,
						language.NewGameOverwriteConfirm.Title,
						StartNewGame,
						() => OnShow(false)
					)
				);
			}
			else StartNewGame();
		}

		void OnContinueGameClick()
		{
			OnLoadGame(RequestResult.Success(), payload.ContinueSave);
		}

		void OnSettingsClick()
		{
			OnNotImplimentedClick();
		}

		void OnCreditsClick()
		{
			OnNotImplimentedClick();
		}

		void OnQuitClick()
		{
			App.Callbacks.DialogRequest(
				DialogRequest.ConfirmDeny(
					language.QuitConfirm.Message,
					DialogStyles.Warning,
					language.QuitConfirm.Title,
					PushQuitRequest,
					() => OnShow(false)
				)
			);
		}

		void OnNotImplimentedClick()
		{
			App.Callbacks.DialogRequest(
				DialogRequest.Confirm(
					LanguageStringModel.Override("This feature is not implemented yet."),
					DialogStyles.Warning,
					confirmClick: () => OnShow(false)
				)
			);
		}
		#endregion

		#region Game Utility
		void StartNewGame()
		{
			App.GameService.CreateGame(payload.NewGameBlock, OnStartNewGame);
		}

		void OnStartNewGame(RequestResult result, GameModel model)
		{
			if (result.IsNotSuccess)
			{
				App.Callbacks.DialogRequest(
					DialogRequest.Confirm(
						language.NewGameError.Message,
						DialogStyles.Error,
						language.NewGameError.Title,
						() => OnShow(false)
					)
				);
				return;
			}
			StartGame(model);
		}

		void OnLoadGame(RequestResult result, GameModel model)
		{
			if (result.IsNotSuccess)
			{
				App.Callbacks.DialogRequest(
					DialogRequest.Confirm(
						language.ContinueGameError.Message,
						DialogStyles.Error,
						language.ContinueGameError.Title,
						() => OnShow(false)
					)
				);
				return;
			}
			StartGame(model);
		}

		void StartGame(GameModel model)
		{
			SM.PushBlocking(
				done => App.Callbacks.SetFocusRequest(SetFocusRequest.Request(HomeState.Focuses.GetNoFocus(), done)),
				"StartGameSetNoFocus"
			);

			SM.PushBlocking(
				done => App.Callbacks.CameraMaskRequest(CameraMaskRequest.Hide(payload.MenuAnimationMultiplier * CameraMaskRequest.DefaultHideDuration, done)),
				"StartGameHideCamera"
			);

			SM.Push(
				() =>
				{
					Debug.Log("Starting game...");
					var gamePayload = new GamePayload
					{
						MainCamera = payload.MainCamera,
						Game = model
					};
				App.SM.RequestState(gamePayload);
				},
				"StartGameRequestState"
			);
		}
		#endregion
	}
}