﻿using System;
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

		GamePayload payload;
		GameModel model;
		GameCompleteLanguageBlock language;

		public GameCompletePresenter(
			GamePayload payload,
			GameModel model,
			GameCompleteLanguageBlock language
		)
		{
			this.payload = payload;
			this.model = model;
			this.language = language;

			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}

		void Show(
			EncounterEvents.GameComplete.Conditions condition,
			KeyValueListModel keyValues
		)
		{
			model.Context.PauseMenuBlockers.Value++;

			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));

			View.Reset();

			var entries = new List<IVerticalOptionsEntry>();

			Debug.Log("a game over event was recieved with condition: " + condition);

			var title = keyValues.GetString(EncounterEvents.GameComplete.StringKeys.Title);
			var header = keyValues.GetString(EncounterEvents.GameComplete.StringKeys.Header);
			var body = keyValues.GetString(EncounterEvents.GameComplete.StringKeys.Body);

			var defaultTitle = "< undefined title >";
			var defaultHeader = "< undefined header >";
			var defaultBody = "< undefined body >";

			switch (condition)
			{
				case EncounterEvents.GameComplete.Conditions.Success:
					defaultTitle = language.SuccessTitle.Value;
					defaultHeader = language.SuccessHeader.Value;
					defaultBody = language.SuccessBody.Value;
					break;
				case EncounterEvents.GameComplete.Conditions.Failure:
					defaultTitle = language.FailureTitle.Value;
					defaultHeader = language.FailureHeader.Value;
					defaultBody = language.FailureBody.Value;
					break;
				default:
					Debug.LogError("Unrecognized Condition: " + condition);
					break;
			}

			var theme = condition == EncounterEvents.GameComplete.Conditions.Success ? VerticalOptionsThemes.Success : VerticalOptionsThemes.Error;
			var icon = condition == EncounterEvents.GameComplete.Conditions.Success ? VerticalOptionsIcons.GameSuccess : VerticalOptionsIcons.GameFailure;

			title = string.IsNullOrEmpty(title) ? defaultTitle : title;
			header = string.IsNullOrEmpty(header) ? defaultHeader : header;
			body = string.IsNullOrEmpty(body) ? defaultBody : body;

			if (title == GDCHackGlobals.SettlementChosenTrigger)
			{
				title = "replacement here";
			}

			if (!string.IsNullOrEmpty(title)) entries.Add(LabelVerticalOptionsEntry.CreateTitle(title, icon));
			if (!string.IsNullOrEmpty(header)) entries.Add(LabelVerticalOptionsEntry.CreateHeader(header));
			if (!string.IsNullOrEmpty(body)) entries.Add(LabelVerticalOptionsEntry.CreateBody(body));

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

			App.Analytics.GameOver(
				model,
				condition,
				title
			);

			ShowView();
		}

		void Close()
		{
			CloseView();

			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetToolbarSelectionFocus(model.ToolbarSelection.Value)));
		}

		bool NotInteractable { get { return View.TransitionState != TransitionStates.Shown; } }

		void PushCloseRequest(string description)
		{
			SM.PushBlocking(
				done =>
				{
					View.Closed += done;
					CloseView();
				},
				description
			);
		}

		void PushMainMenuRequest(string description, bool autoNewgame = false)
		{
			SM.Push(
				() =>
				{
					var homePayload = new HomePayload();
					homePayload.MainCamera = payload.MainCamera;
					homePayload.AutoRetryNewGame = autoNewgame;
					App.SM.RequestState(homePayload);
				},
				description
			);
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

			var condition = gameCompletion.KeyValues.GetEnum<EncounterEvents.GameComplete.Conditions>(EncounterEvents.GameComplete.EnumKeys.Condition);

			var details = model.SaveDetails.Value;
			details.IsCompleted = true;
			details.CompleteCondition = condition;
			details.CompleteKeyValues = gameCompletion.KeyValues;

			model.SaveDetails.Value = details;

			Show(condition, gameCompletion.KeyValues);
		}
		#endregion

		#region Click Events
		void OnClickRetry()
		{
			if (NotInteractable) return;

			PushCloseRequest("ClosingCompletionForRetry");

			SM.PushBlocking(
				done =>
				{
					View.Reset();
					View.Shown += done;
					View.SetEntries(
						VerticalOptionsThemes.Neutral,
						LabelVerticalOptionsEntry.CreateTitle(language.RetryTitle.Value, VerticalOptionsIcons.Return)
					);
					ShowView();
				},
				"ShowingCompletionRetry"
			);

			SM.PushBlocking(
				done => App.Heartbeat.Wait(done, 0.5f),
				"WaitingCompletionForMinimumRetry"
			);

			PushMainMenuRequest("RequestingMainMenuFromCompletionForRetry", true);
		}

		void OnClickMainMenu()
		{
			if (NotInteractable) return;

			PushCloseRequest("ClosingCompletionForMainMenu");

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
				"ShowingCompletionMainMenu"
			);

			PushMainMenuRequest("RequestingMainMenuFromCompletion");
		}
		#endregion

	}
}