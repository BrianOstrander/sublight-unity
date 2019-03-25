using System;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class PauseMenuPresenter : VerticalOptionsPresenter
	{
		enum States
		{
			Unknown = 0,
			Default = 10,
			Animating = 20
		}

		GamePayload payload;
		GameModel model;
		PauseMenuLanguageBlock language;
		PreferencesPresenter preferences;

		States state = States.Default;
		DialogRequest.States lastDialogState = DialogRequest.States.Complete;

		SaveRequest? saveResult;
		Action popElapsedTimeBlock;
		Action popHasSavedBlock;
		bool HasSavedSinceOpening { get { return popHasSavedBlock != null; } }

		public PauseMenuPresenter(
			GamePayload payload,
			GameModel model,
			PauseMenuLanguageBlock language,
			PreferencesPresenter preferences
		)
		{
			this.payload = payload;
			this.model = model;
			this.language = language;
			this.preferences = preferences;

			App.Callbacks.DialogRequest += OnDialogRequest;
			App.Callbacks.Escape += OnEscape;

			model.Context.PauseMenuBlockers.Changed += OnPauseMenuBlockers;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.DialogRequest -= OnDialogRequest;
			App.Callbacks.Escape -= OnEscape;

			model.Context.PauseMenuBlockers.Changed -= OnPauseMenuBlockers;
		}

		string SaveMessage
		{
			get
			{
				var elapsedResult = string.Empty;
				var elapsedSinceSave = model.ElapsedTime.Value - model.SaveDetails.Value.ElapsedTime;

				if (elapsedSinceSave < TimeSpan.FromMinutes(1f)) elapsedResult = "less than a minute ago";
				else if (elapsedSinceSave < TimeSpan.FromMinutes(3f)) elapsedResult = "a couple minutes ago";
				else if (elapsedSinceSave < TimeSpan.FromMinutes(6f)) elapsedResult = "5 minutes ago";
				else if (elapsedSinceSave < TimeSpan.FromHours(1f)) elapsedResult = elapsedSinceSave.TotalMinutes.ToString("N0") + " minutes ago";
				else elapsedResult = elapsedSinceSave.TotalHours.ToString("N0") + " hours and " + elapsedSinceSave.Minutes.ToString("N0") + " minutes ago";

				var currentTransit = model.TransitHistory.Peek();
				var lastTransit = currentTransit.TransitCount == 0 ? currentTransit : model.TransitHistory.Peek(1);
				var saveTransit = model.TransitHistory.Peek(model.SaveDetails.Value.TransitHistoryId);

				var transitResult = string.Empty;

				if (model.SaveDetails.Value.TransitHistoryId == currentTransit.Id) transitResult = "in the current system";
				else if (model.SaveDetails.Value.TransitHistoryId == lastTransit.Id) transitResult = "in the previous system";
				else transitResult = (currentTransit.TransitCount - saveTransit.TransitCount) + " transits previous in the " + saveTransit.SystemName + " system";

				return "Last saved " + elapsedResult + ", " + transitResult + ".";
			}
		}

		bool EscapeEnabled
		{
			get
			{
				return state == States.Default &&
					                  lastDialogState == DialogRequest.States.Complete &&
					                  model.Context.PauseMenuBlockers.Value <= 0 &&
					                  model.Context.TransitState.Value.State == TransitState.States.Complete &&
					                  (model.Context.ToolbarSelectionRequest.Value.Selection == model.ToolbarSelection.Value || model.Context.ToolbarSelectionRequest.Value.Selection == ToolbarSelections.Unknown);
			}
		}

		bool NotInteractable
		{
			get
			{
				return state != States.Default || View.TransitionState != TransitionStates.Shown;
			}
		}

		bool WarnBeforeLeaving
		{
			get
			{
				return false;
			}
		}

		void PushMainMenuRequest()
		{
			// Pause menu should be close should already be closed or on the state machine's stack.

			SM.PushBlocking(
				done =>
				{
					View.Reset();
					View.Shown += done;
					View.SetEntries(
						VerticalOptionsThemes.Neutral,
						LabelVerticalOptionsEntry.CreateTitle(language.ReturningToMainMenu.Value, VerticalOptionsIcons.Return)
					);
					ShowView();
				},
				"ShowingPauseMenuReturningToMainMenu"
			);

			SM.PushBlocking(
				done => App.Heartbeat.Wait(done, 0.5f),
				"WaitingPauseMenuForMinimumReturningToMainMenuTime"
			);

			SM.Push(
				() =>
				{
					var homePayload = new HomePayload();
					homePayload.MainCamera = payload.MainCamera;
					App.SM.RequestState(homePayload);
				},
				"RequestingMainMenuFromPauseMenu"
			);
		}

		void PushQuitRequest()
		{
			// Pause menu should be close should already be closed or on the state machine's stack.

			SM.PushBlocking(
				done =>
				{
					View.Reset();
					View.Shown += done;
					View.SetEntries(
						VerticalOptionsThemes.Error,
						LabelVerticalOptionsEntry.CreateTitle(language.Quiting.Value, VerticalOptionsIcons.Quit)
					);
					ShowView();
				},
				"ShowingPauseMenuQuitting"
			);

			SM.PushBlocking(
				done => App.Heartbeat.Wait(done, 0.5f),
				"WaitingPauseMenuForMinimumQuittingTime"
			);

			SM.Push(
				() =>
				{
					App.SM.RequestState(
						TransitionPayload.Quit("GamePauseMenu")
					);
				},
				"QuittingFromPauseMenu"
			);
		}

		void Show(bool instant = false)
		{
			popElapsedTimeBlock = model.Context.ElapsedTimeBlockers.Push("Pause Menu Open");
			saveResult = null;
			state = States.Default;

			if (instant) App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));
			else App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));

			View.Reset();

			ButtonVerticalOptionsEntry saveEntry = null;

			if (model.Context.SaveBlockers.None) saveEntry = ButtonVerticalOptionsEntry.CreateButton(language.Save.Value, OnClickSave);
			else saveEntry = ButtonVerticalOptionsEntry.CreateButton(language.Save.Value, OnClickSaveDisabled, ButtonVerticalOptionsEntry.InteractionStates.LooksNotInteractable);

			View.SetEntries(
				VerticalOptionsThemes.Neutral,
				LabelVerticalOptionsEntry.CreateTitle(language.Title.Value, VerticalOptionsIcons.Pause),
				LabelVerticalOptionsEntry.CreateHeader(SaveMessage),
				ButtonVerticalOptionsEntry.CreateButton(language.Resume.Value, OnClickResume),
				saveEntry,
				ButtonVerticalOptionsEntry.CreateButton(language.Preferences.Value, OnClickPreferences),
				ButtonVerticalOptionsEntry.CreateButton(language.MainMenu.Value, OnClickMainMenu),
				ButtonVerticalOptionsEntry.CreateButton(language.Quit.Value, OnClickQuit)
			);

			App.Analytics.ScreenVisit(AnalyticsService.ScreenNames.PauseMenu);

			ShowView(instant: instant);
		}

		void Close()
		{
			popElapsedTimeBlock();
			if (popHasSavedBlock != null) popHasSavedBlock();

			CloseView();

			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetToolbarSelectionFocus(model.ToolbarSelection.Value)));
		}

		#region Events
		void OnDialogRequest(DialogRequest request)
		{
			lastDialogState = request.State;
		}

		void OnEscape()
		{
			if (!EscapeEnabled) return;

			switch (View.TransitionState)
			{
				case TransitionStates.Closed:
					Show();
					break;
				case TransitionStates.Shown:
					Close();
					break;
			}
		}

		void OnPauseMenuBlockers(int count)
		{
			if (count < 0) Debug.LogError("Pause Menu Blockers is less than zero, this should never happen!");
		}

		void OnCheckSaveResult()
		{
			// When this method is called, a "Saving..." message should still be on screen.

			var result = saveResult.Value;
			if (result.State != SaveRequest.States.Complete) Debug.LogError("Checking save result with state "+result.State+", unpredictable behaviour may occur");

			if (result.Status == RequestStatus.Success) OnSaveResultSuccess();
			else OnSaveResultError();
		}

		void OnSaveResultSuccess()
		{
			// When this method is called, a "Saving..." message should still be on screen.
			popHasSavedBlock = model.Context.SaveBlockers.Push(language.SaveDisabledAlreadySaved);

			SM.Push(
				() =>
				{
					View.Reset();
					View.SetEntries(
						VerticalOptionsThemes.Warning,
						LabelVerticalOptionsEntry.CreateTitle(language.SavingComplete.Value, VerticalOptionsIcons.Save)
					);
					ShowView(instant: true);
				},
				"ShowingPauseMenuForSavingComplete"
			);

			SM.PushBlocking(
				done => App.Heartbeat.Wait(done, 0.5f),
				"WaitingPauseMenuForMinimumSavingCompleteTime"
			);

			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingPauseMenuFromSavingComplete"
			);

			SM.Push(
				() => Show(),
				"ShowingPauseMenuFromSavingComplete"
			);
		}

		void OnSaveResultError()
		{
			// When this method is called, a "Saving..." message should still be on screen.

			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingPauseMenuFromSavingError"
			);

			SM.PushBlocking(
				done =>
				{
					App.Callbacks.DialogRequest(
						DialogRequest.Confirm(
							language.SavingError.Message,
							DialogStyles.Error,
							language.SavingError.Title,
							done,
							overrideFocuseHandling: true
						)
					);
				},
				"ShowingDialogFromSaveError"
			);

			SM.Push(
				() => Show(),
				"ShowingPauseMenuFromSaveErrorDialog"
			);
		}
		#endregion

		#region Pause Menu Click Events
		void OnClickResume()
		{
			if (NotInteractable) return;
			Close();
		}

		void OnClickSave()
		{
			if (NotInteractable) return;

			state = States.Animating;

			App.Callbacks.SaveRequest(SaveRequest.Request(result => saveResult = result));

			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingPauseMenuForSaving"
			);

			SM.PushBlocking(
				done =>
				{
					View.Reset();
					View.Shown += done;
					View.SetEntries(
						VerticalOptionsThemes.Warning,
						LabelVerticalOptionsEntry.CreateTitle(language.SavingTitle.Value, VerticalOptionsIcons.Save)
					);
					ShowView();
				},
				"ShowingPauseMenuForSaving"
			);

			SM.PushBlocking(
				done => App.Heartbeat.Wait(done, 0.5f),
				"WaitingPauseMenuForMinimumSavingTime"
			);

			SM.PushBlocking(
				() => saveResult.HasValue,
				"WaitingPauseMenuForSavingResult"
			);

			SM.Push(OnCheckSaveResult, "PauseMenuCheckSaveResult");
		}

		void OnClickSaveDisabled()
		{
			if (NotInteractable) return;

			state = States.Animating;

			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingPauseMenuFromSaveDisabled"
			);

			var currentBlocker = model.Context.SaveBlockers.Peek();

			SM.PushBlocking(
				done =>
				{
					App.Callbacks.DialogRequest(
						DialogRequest.Confirm(
							currentBlocker.Message,
							DialogStyles.Error,
							currentBlocker.Title,
							done,
							overrideFocuseHandling: true
						)
					);
				},
				"ShowingDialogFromSaveDisabled"
			);

			SM.Push(
				() => Show(),
				"ShowingPauseMenuFromSaveDisabled"
			);
		}

		void OnClickPreferences()
		{
			if (NotInteractable) return;

			model.Context.PauseMenuBlockers.Value++;

			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingPauseMenuForPreferences"
			);

			SM.Push(
				() => preferences.Show(
					ActionExtensions.GetEmpty<bool>(),
					() =>
					{
						Show();
						model.Context.PauseMenuBlockers.Value--;
					},
					reFocus: false
				),
				"ShowingPreferencesFromPauseMenu"
			);
		}

		void OnClickMainMenu()
		{
			if (NotInteractable) return;

			state = States.Animating;

			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingPauseMenuForMainMenu"
			);

			if (HasSavedSinceOpening)
			{
				PushMainMenuRequest();
				return;
			}

			SM.PushBlocking(
				done =>
				{
					App.Callbacks.DialogRequest(
						DialogRequest.ConfirmDeny(
							language.MainMenuConfirm.Message,
							DialogStyles.Error,
							language.MainMenuConfirm.Title,
							PushMainMenuRequest,
							() => Show(),
							done: result => done(),
							overrideFocuseHandling: true
						)
					);
				},
				"ShowingDialogFromMainMenuClick"
			);
		}

		void OnClickQuit()
		{
			if (NotInteractable) return;

			state = States.Animating;

			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingPauseMenuForQuit"
			);

			if (HasSavedSinceOpening)
			{
				PushQuitRequest();
				return;
			}

			SM.PushBlocking(
				done =>
				{
					App.Callbacks.DialogRequest(
						DialogRequest.ConfirmDeny(
							language.QuitConfirm.Message,
							DialogStyles.Error,
							language.QuitConfirm.Title,
							PushQuitRequest,
							() => Show(),
							done: result => done(),
							overrideFocuseHandling: true
						)
					);
				},
				"ShowingDialogFromQuitClick"
			);
		}
		#endregion

	}
}