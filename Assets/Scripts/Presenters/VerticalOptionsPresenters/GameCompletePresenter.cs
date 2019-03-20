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
				var game = (App.SM.CurrentHandler as GameState).Payload.Game;
				var systemModel = game.Context.CurrentSystem.Value;

				var newMessage = string.Empty;

				var planetReadable = GDCHackGlobals.PlanetPositionReadable(systemModel.KeyValues.Get(KeyDefines.CelestialSystem.PlanetCount));
				newMessage += "We've sent a probe to the surface, here are the results.\n\n";

				var scanLevel = game.KeyValues.Get(KeyDefines.Game.SurfaceProbeScanLevel);

				var systemScanLevelAtmosphere = 0;
				var systemScanLevelGravity = 0;
				var systemScanLevelTemperature = 0;
				var systemScanLevelWater = 0;
				var systemScanLevelResources = 0;

				var systemAtmosphere = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableAtmosphere);
				var systemGravity = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableGravity);
				var systemTemperature = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableTemperature);
				var systemWater = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableWater);
				var systemResources = systemModel.KeyValues.Get(KeyDefines.CelestialSystem.HabitableResources);

				var finalScore = systemAtmosphere + systemGravity + systemTemperature + systemWater + systemResources;



				var allScanLevels = new int[]
				{
					systemScanLevelAtmosphere,
					systemScanLevelGravity,
					systemScanLevelTemperature,
					systemScanLevelWater,
					systemScanLevelResources
				};

				var maxScanLevel = Mathf.Max(allScanLevels);

				var anyScansObscured = 0 < maxScanLevel;
				var anyScanOutOfRange = scanLevel < maxScanLevel;
				var anyScansObscuredButScannable = allScanLevels.Any(s => 0 < s && s <= scanLevel);
				var allScansAreInRange = allScanLevels.None(s => scanLevel < s);

				var probesLeft = game.KeyValues.Get(KeyDefines.Game.SurfaceProbeCount);
				//var anyProbesLeft = 0 < probesLeft;

				newMessage += GDCHackGlobals.GetReading(
					systemAtmosphere,
					scanLevel,
					systemScanLevelAtmosphere,
					GDCHackGlobals.HabitableAtmosphereDescriptions,
					true,
					DeveloperStrings.GetBold("Atmosphere:")
				) + "\n";
				newMessage += GDCHackGlobals.GetReading(
					systemGravity,
					scanLevel,
					systemScanLevelGravity,
					GDCHackGlobals.HabitableGravityDescriptions,
					true,
					DeveloperStrings.GetBold("Gravity:\t")
				) + "\n";
				newMessage += GDCHackGlobals.GetReading(
					systemTemperature,
					scanLevel,
					systemScanLevelTemperature,
					GDCHackGlobals.HabitableTemperatureDescriptions,
					true,
					DeveloperStrings.GetBold("Temperature:")
				) + "\n";
				newMessage += GDCHackGlobals.GetReading(
					systemWater,
					scanLevel,
					systemScanLevelWater,
					GDCHackGlobals.HabitableWaterDescriptions,
					true,
					DeveloperStrings.GetBold("Water:\t")
				) + "\n";
				newMessage += GDCHackGlobals.GetReading(
					systemResources,
					scanLevel,
					systemScanLevelResources,
					GDCHackGlobals.HabitableResourcesDescriptions,
					true,
					DeveloperStrings.GetBold("Resources:\t")
				) + "\n";

				// var min = 0, max = 20

				newMessage += "\n";

				if (finalScore == 0)
				{
					title = "A Hellish World";
					newMessage += "You've chosen a hellish world, your people are doomed.";
				}
				else if (finalScore < 5)
				{
					title = "A Tough World";
					newMessage += "You've chosen a nearly impossible to settle world, life will be tough for the few who survive.";
				}
				else if (finalScore < 10)
				{
					title = "Better Than Nothing";
					newMessage += "The world you've chosen is no paradise, but it's better than the cold vacuum of space...";
				}
				else if (finalScore < 15)
				{
					title = "It Will Do";
					newMessage += "You've chosen a world with a hospitable environment. With enough work, it's something we could call home.";
				}
				else if (finalScore == 20)
				{
					title += "A Perfect World";
					newMessage += "This planet is such a jewel, how could we have been so lucky to find it?!";
				}
				else
				{
					title = "This Planet is a Treasure";
					newMessage += "Your people step off their landers to find a lush world, ripe for settlement. We will live long and happy lives here...";
				}

				newMessage += "\n\nFinal Score: " + DeveloperStrings.GetBold((finalScore * 10) + " / 200");

				body = newMessage;
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