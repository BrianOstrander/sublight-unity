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
		LearnMorePresenter learnMore;

		public MainMenuOptionsPresenter(
			HomePayload payload,
			MainMenuLanguageBlock language,
			PreferencesPresenter preferences,
			LearnMorePresenter learnMore
		)
		{
			this.payload = payload;
			this.language = language;
			this.preferences = preferences;
			this.learnMore = learnMore;
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
				new LabelButtonBlock(language.LearnMore, CloseThenClick(OnLearnMoreClick)),
				//new LabelButtonBlock(language.Credits, CloseThenClick(OnCreditsClick)),
				new LabelButtonBlock(language.Quit, CloseThenClick(OnQuitClick))
			};

			App.Analytics.ScreenVisit(AnalyticsService.ScreenNames.MainMenu);

			ShowView(parent, instant);
		}

		void PushQuitRequest()
		{
			SM.Push(
				() =>
				{
					App.SM.RequestState(
						TransitionPayload.Quit("HomeMainMenu")
					);
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
			if (!App.MetaKeyValues.Get(KeyDefines.Preferences.IsDemoMode) && payload.CanContinueSave)
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
			OnShowContextualOptions(preferences);
		}

		void OnFeedbackClick()
		{
			App.Heartbeat.Wait(
				() => 
				{
					Application.OpenURL(
						App.BuildPreferences.FeedbackForm(
							FeedbackFormTriggers.MainMenu,
							App.MetaKeyValues.GlobalKeyValues
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

		void OnLearnMoreClick()
		{
			OnShowContextualOptions(learnMore);
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

		void OnShowContextualOptions(ContextualOptionsPresenter presenter)
		{
			presenter.Show(
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
			payload.StartGame(model, false, false);
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
			payload.StartGame(model, false, true);
		}
		#endregion
	}
}