using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class GameCompletePresenter : VerticalOptionsPresenter
	{
		enum States
		{
			Unknown = 0,
			Default = 10,
			Animating = 20
		}

		GameModel model;
		GameCompleteLanguageBlock language;

		public GameCompletePresenter(
			GameModel model,
			GameCompleteLanguageBlock language
		)
		{
			this.model = model;
			this.language = language;

			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}

		/*
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
		*/

		void Show(
			KeyValueListModel keyValues
		)
		{
			model.Context.PauseMenuBlockers.Value++;

			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));

			View.Reset();

			var entries = new List<IVerticalOptionsEntry>();

			var condition = keyValues.GetEnum<EncounterEvents.GameComplete.Conditions>(EncounterEvents.GameComplete.EnumKeys.Condition);
			Debug.Log("a game over event was recieved with condition: " + condition);

			var title = keyValues.GetString(EncounterEvents.GameComplete.StringKeys.Title);
			var message = keyValues.GetString(EncounterEvents.GameComplete.StringKeys.Message);

			var defaultTitle = "< undefined title >";
			var defaultMessage = "< undefined message >";

			switch (condition)
			{
				case EncounterEvents.GameComplete.Conditions.Success:
					defaultTitle = language.SuccessTitle.Value;
					defaultMessage = language.SuccessMessage.Value;
					break;
				case EncounterEvents.GameComplete.Conditions.Failure:
					defaultTitle = language.FailureTitle.Value;
					defaultMessage = language.FailureMessage.Value;
					break;
				default:
					Debug.LogError("Unrecognized Condition: " + condition);
					break;
			}

			var theme = condition == EncounterEvents.GameComplete.Conditions.Success ? VerticalOptionsThemes.Success : VerticalOptionsThemes.Error;
			var icon = condition == EncounterEvents.GameComplete.Conditions.Success ? VerticalOptionsIcons.GameSuccess : VerticalOptionsIcons.GameFailure;

			entries.AddRange(
				new IVerticalOptionsEntry[]
				{
					LabelVerticalOptionsEntry.CreateTitle(string.IsNullOrEmpty(title) ? defaultTitle : title, icon),
					LabelVerticalOptionsEntry.CreateHeader(string.IsNullOrEmpty(message) ? defaultMessage : message)
				}
			);

			switch (condition)
			{
				case EncounterEvents.GameComplete.Conditions.Failure:
					entries.Add(
						ButtonVerticalOptionsEntry.CreateButton(language.Retry.Value, OnClickRetry)
					);
					break;
			}

			entries.Add(
				ButtonVerticalOptionsEntry.CreateButton(language.MainMenu.Value, OnClickMainMenu)
			);

			View.SetEntries(
				theme,
				entries.ToArray()
			);

			ShowView();
		}

		void Close()
		{
			CloseView();

			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetToolbarSelectionFocus(model.ToolbarSelection.Value)));
		}

		#region Events
		void OnEncounterRequest(EncounterRequest request)
		{
			switch (request.State)
			{
				case EncounterRequest.States.Handle:
					request.TryHandle<EncounterEventHandlerModel>(OnHandleEvent);
					break;
			}
		}

		public void OnHandleEvent(EncounterEventHandlerModel handler)
		{
			// They should already be filtered at this point...
			var gameCompletion = handler.Events.Value.FirstOrDefault(e => e.EncounterEvent.Value == EncounterEvents.Types.GameComplete);
			if (gameCompletion == null) return;

			Show(gameCompletion.KeyValues);
		}
		#endregion

		#region Click Events
		void OnClickRetry()
		{
			Debug.Log("click retry");
		}

		void OnClickMainMenu()
		{
			Debug.Log("click main menu");
		}
		#endregion

	}
}