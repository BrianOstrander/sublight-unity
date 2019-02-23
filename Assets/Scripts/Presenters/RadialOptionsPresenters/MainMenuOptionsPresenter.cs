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
		PreferencesPresenter preferences;

		public MainMenuOptionsPresenter(
			HomePayload payload,
			MainMenuLanguageBlock language,
			PreferencesPresenter preferences
		)
		{
			this.payload = payload;
			this.language = language;
			this.preferences = preferences;
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
				new LabelButtonBlock(language.Preferences, CloseThenClick(OnPreferencesClick)),
				new LabelButtonBlock(language.Feedback, CloseThenClick(OnFeedbackClick)),
				//new LabelButtonBlock(language.Credits, CloseThenClick(OnCreditsClick)),
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

		void OnPreferencesClick()
		{
			preferences.Show(
				setFocusInstant =>
				{
					if (setFocusInstant) App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(HomeState.Focuses.GetPriorityFocus()));
					else App.Callbacks.SetFocusRequest(SetFocusRequest.Request(HomeState.Focuses.GetPriorityFocus()));
				},
				() =>
				{
					App.Callbacks.SetFocusRequest(SetFocusRequest.Request(HomeState.Focuses.GetMainMenuFocus()));
					OnShow(false);
				}
			);
		}

		void OnFeedbackClick()
		{
			KeyValueListModel globalSource = null;

			if (App.MetaKeyValues != null && App.MetaKeyValues.GlobalKeyValues != null)
			{
				globalSource = App.MetaKeyValues.GlobalKeyValues.KeyValues;
			}

			App.Heartbeat.Wait(
				() => 
				{
					Application.OpenURL(
						App.BuildPreferences.FeedbackForm(
							FeedbackFormTriggers.MainMenu,
							globalSource
						)
					);
				},
				0.75f
			);

			App.Callbacks.DialogRequest(
				DialogRequest.Confirm(
					LanguageStringModel.Override("Your browser should open to a feedback form, if not visit <b>strangestar.games/contact</b> to send us a message!"),
					DialogStyles.Neutral,
					LanguageStringModel.Override("Feedback"),
					confirmClick: () => OnShow(false)
				)
			);
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
			payload.StartGame(model);
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
			payload.StartGame(model);
		}
		#endregion
	}
}