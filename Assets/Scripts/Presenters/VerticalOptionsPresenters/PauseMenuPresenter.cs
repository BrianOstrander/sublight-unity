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

		States state = States.Default;
		DialogRequest.States lastDialogState = DialogRequest.States.Complete;

		bool hasSavedSinceOpening;
		SaveRequest? saveResult;

		public PauseMenuPresenter(
			GamePayload payload,
			GameModel model,
			PauseMenuLanguageBlock language
		)
		{
			this.payload = payload;
			this.model = model;
			this.language = language;

			App.Callbacks.DialogRequest += OnDialogRequest;
			App.Callbacks.Escape += OnEscape;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.DialogRequest -= OnDialogRequest;
			App.Callbacks.Escape -= OnEscape;
		}

		string SaveMessage
		{
			get
			{
				return "Last saved 5 minutes ago, in the current system";
			}
		}

		bool EscapeEnabled
		{
			get
			{
				return state == States.Default &&
					                  lastDialogState == DialogRequest.States.Complete &&
					                  model.Context.TransitState.Value.State == TransitState.States.Complete;
			}
		}

		bool NotInteractable
		{
			get
			{
				return state != States.Default || View.TransitionState != TransitionStates.Shown;
			}
		}

		bool CanSave
		{
			get
			{
				return model.Context.TransitState.Value.State == TransitState.States.Complete &&
					        model.Context.EncounterState.Current.Value.State == EncounterStateModel.States.Complete &&
					        !hasSavedSinceOpening;
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
					var quitPayload = new QuitPayload();
					quitPayload.Requester = "GamePauseMenu";
					App.SM.RequestState(quitPayload);
				},
				"QuittingFromPauseMenu"
			);
		}

		void Show(bool instant = false)
		{
			saveResult = null;
			state = States.Default;

			if (instant) App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));
			else App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));

			View.Reset();

			ButtonVerticalOptionsEntry saveEntry = null;

			if (CanSave) saveEntry = ButtonVerticalOptionsEntry.CreateButton(language.Save.Value, OnClickSave);
			else saveEntry = ButtonVerticalOptionsEntry.CreateButton(language.Save.Value, OnClickSaveDisabled, ButtonVerticalOptionsEntry.InteractionStates.LooksNotInteractable);

			View.SetEntries(
				VerticalOptionsThemes.Neutral,
				LabelVerticalOptionsEntry.CreateTitle(language.Title.Value, VerticalOptionsIcons.Pause),
				LabelVerticalOptionsEntry.CreateHeader(SaveMessage),
				ButtonVerticalOptionsEntry.CreateButton(language.Resume.Value, OnClickResume),
				saveEntry,
				ButtonVerticalOptionsEntry.CreateButton(language.MainMenu.Value, OnClickMainMenu),
				ButtonVerticalOptionsEntry.CreateButton(language.Quit.Value, OnClickQuit)
			);

			ShowView(instant: instant);
		}

		void Close()
		{
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
					hasSavedSinceOpening = false;
					Show();
					break;
				case TransitionStates.Shown:
					Close();
					break;
			}
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
			hasSavedSinceOpening = true;

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
							true
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

			if (hasSavedSinceOpening)
			{
				SM.PushBlocking(
					done =>
					{
						View.Closed += done;
						CloseView();
					},
					"ClosingPauseMenuFromAlreadySavedError"
				);

				SM.PushBlocking(
					done =>
					{
						App.Callbacks.DialogRequest(
							DialogRequest.Confirm(
								language.SaveDisabledAlreadySaved.Message,
								DialogStyles.Error,
								language.SaveDisabledAlreadySaved.Title,
								done,
								true
							)
						);
					},
					"ShowingDialogFromAlreadySavedError"
				);

				SM.Push(
					() => Show(),
					"ShowingPauseMenuFromAlreadySavedErrorDialog"
				);

				return;
			}
			if (model.Context.EncounterState.Current.Value.State != EncounterStateModel.States.Complete)
			{
				SM.PushBlocking(
					done =>
					{
						View.Closed += done;
						CloseView();
					},
					"ClosingPauseMenuFromEncounterSaveError"
				);

				SM.PushBlocking(
					done =>
					{
						App.Callbacks.DialogRequest(
							DialogRequest.Confirm(
								language.SaveDisabledDuringEncounter.Message,
								DialogStyles.Error,
								language.SaveDisabledDuringEncounter.Title,
								done,
								true
							)
						);
					},
					"ShowingDialogFromEncounterSaveError"
				);

				SM.Push(
					() => Show(),
					"ShowingPauseMenuFromEncounterSaveError"
				);
				return;
			}

			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingPauseMenuFromUnknownSaveError"
			);

			SM.PushBlocking(
				done =>
				{
					App.Callbacks.DialogRequest(
						DialogRequest.Confirm(
							language.SaveDisabledUnknown.Message,
							DialogStyles.Error,
							language.SaveDisabledUnknown.Title,
							done,
							true
						)
					);
				},
				"ShowingDialogFromUnknownSaveError"
			);

			SM.Push(
				() => Show(),
				"ShowingPauseMenuFromUnknownSaveError"
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

			if (hasSavedSinceOpening)
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
							result => done(),
							true
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

			if (hasSavedSinceOpening)
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
							result => done(),
							true
						)
					);
				},
				"ShowingDialogFromQuitClick"
			);
		}
		#endregion

	}
}