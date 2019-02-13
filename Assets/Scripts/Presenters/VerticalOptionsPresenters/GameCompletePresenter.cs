using System;
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

		public GameCompletePresenter(
			GameModel model
		)
		{
			this.payload = payload;
			this.model = model;

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

		void Show()
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));

			View.Reset();

			View.SetEntries(
				VerticalOptionsThemes.Neutral
				//LabelVerticalOptionsEntry.CreateTitle(language.Title.Value, VerticalOptionsIcons.Pause),
				//LabelVerticalOptionsEntry.CreateHeader(SaveMessage),
				//ButtonVerticalOptionsEntry.CreateButton(language.Resume.Value, OnClickResume),
				//saveEntry,
				//ButtonVerticalOptionsEntry.CreateButton(language.MainMenu.Value, OnClickMainMenu),
				//ButtonVerticalOptionsEntry.CreateButton(language.Quit.Value, OnClickQuit)
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

		public static void OnHandleEvent(EncounterEventHandlerModel handler)
		{
			// They should already be filtered at this point...
			var gameCompletion = handler.Events.Value.FirstOrDefault(e => e.EncounterEvent.Value == EncounterEvents.Types.GameComplete);
			if (gameCompletion == null) return;

			var condition = gameCompletion.KeyValues.GetEnum<EncounterEvents.GameComplete.Conditions>(EncounterEvents.GameComplete.EnumKeys.Condition);

			Debug.Log("a game over event was recieved with condition: "+condition);
			//var title = gameCompletion.KeyValues.GetString()
		}
		#endregion

		#region Pause Menu Click Events

		#endregion

	}
}