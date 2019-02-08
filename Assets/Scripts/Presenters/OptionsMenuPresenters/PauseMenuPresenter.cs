using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class PauseMenuPresenter : OptionsMenuPresenter
	{
		enum States
		{
			Unknown = 0,
			Default = 10,
			Animating = 20
		}

		GameModel model;
		PauseMenuLanguageBlock language;

		States state = States.Default;
		DialogRequest.States lastDialogState = DialogRequest.States.Complete;

		bool hasSavedSinceOpening;
		SaveRequest? saveResult;

		public PauseMenuPresenter(
			GameModel model,
			PauseMenuLanguageBlock language
		)
		{
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
				return state == States.Default && lastDialogState == DialogRequest.States.Complete;
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
				return model.TransitState.Value.State == TransitState.States.Complete &&
					        model.EncounterState.Current.Value.State == EncounterStateModel.States.Complete &&
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

		void Show(bool instant = false)
		{
			saveResult = null;
			state = States.Default;

			if (instant) App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));
			else App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));

			View.Reset();

			ButtonOptionsMenuEntry saveEntry = null;

			if (CanSave) saveEntry = ButtonOptionsMenuEntry.CreateButton(language.Save.Value, OnClickSave);
			else saveEntry = ButtonOptionsMenuEntry.CreateButton(language.Save.Value, OnClickSaveDisabled, ButtonOptionsMenuEntry.InteractionStates.LooksNotInteractable);

			View.SetEntries(
				OptionsMenuThemes.Neutral,
				LabelOptionsMenuEntry.CreateTitle(language.Title.Value, OptionsMenuIcons.Pause),
				LabelOptionsMenuEntry.CreateHeader(SaveMessage),
				ButtonOptionsMenuEntry.CreateButton(language.Resume.Value, OnClickResume),
				saveEntry,
				ButtonOptionsMenuEntry.CreateButton(language.MainMenu.Value, OnClickMainMenu),
				ButtonOptionsMenuEntry.CreateButton(language.Quit.Value, OnClickQuit)
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
					View.SetEntries(
						OptionsMenuThemes.Warning,
						LabelOptionsMenuEntry.CreateTitle(language.SavingComplete.Value, OptionsMenuIcons.Save)
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
							language.SavingTitle,
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
					View.Shown += done;
					View.SetEntries(
						OptionsMenuThemes.Warning,
						LabelOptionsMenuEntry.CreateTitle(language.SavingTitle.Value, OptionsMenuIcons.Save)
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

			/*
			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				"ClosingPauseMenuFromSaving"
			);
			*/
			SM.Push(OnCheckSaveResult, "PauseMenuCheckSaveResult");
		}

		void OnClickSaveDisabled()
		{
			if (NotInteractable) return;
			Debug.Log("click save disabled");
		}

		void OnClickMainMenu()
		{
			if (NotInteractable) return;
			Debug.Log("click main menu");
		}

		void OnClickQuit()
		{
			if (NotInteractable) return;
			Debug.Log("click quit");
		}
		#endregion

	}
}