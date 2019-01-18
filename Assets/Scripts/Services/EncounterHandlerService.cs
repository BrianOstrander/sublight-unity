using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class EncounterHandlerService
	{
		Heartbeat heartbeat;
		CallbackService callbacks;
		EncounterService encounterService;
		KeyValueService keyValueService;
		ValueFilterService valueFilter;
		Func<PreferencesModel> currentPreferences;

		EncounterLogModel nextLog;
		float? nextLogDelay;

		EncounterLogHandlerConfiguration configuration;
		IEncounterLogHandler[] handlers;

		public EncounterHandlerService(
			Heartbeat heartbeat,
			CallbackService callbacks,
			EncounterService encounterService,
			KeyValueService keyValueService,
			ValueFilterService valueFilter,
			Func<PreferencesModel> currentPreferences
		)
		{
			if (heartbeat == null) throw new ArgumentNullException("heartbeat");
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (encounterService == null) throw new ArgumentNullException("encounterService");
			if (keyValueService == null) throw new ArgumentNullException("keyValueService");
			if (valueFilter == null) throw new ArgumentNullException("valueFilter");
			if (currentPreferences == null) throw new ArgumentNullException("currentPreferences");

			this.heartbeat = heartbeat;
			this.callbacks = callbacks;
			this.encounterService = encounterService;
			this.keyValueService = keyValueService;
			this.valueFilter = valueFilter;
			this.currentPreferences = currentPreferences;

			this.callbacks.EncounterRequest += OnEncounter;
			this.callbacks.StateChange += OnStateChange;
			this.heartbeat.Update += OnUpdate;

			configuration = new EncounterLogHandlerConfiguration(
				this.callbacks,
				this.encounterService,
				this.keyValueService,
				this.valueFilter,
				this.currentPreferences
			);

			handlers = new IEncounterLogHandler[]
			{
				new TextLogHandler(configuration),
				new KeyValueLogHandler(configuration),
				new SwitchLogHandler(configuration),
				new SwitchLogHandler(configuration),
				new ButtonLogHandler(configuration),
				new EncyclopediaLogHandler(configuration),
				new EncounterEventLogHandler(configuration),
				new DialogLogHandler(configuration)
			};
		}

		void SetOnEncoutnerLast()
		{
			// This is kind of hacky...
			callbacks.EncounterRequest -= OnEncounter;
			callbacks.EncounterRequest += OnEncounter;
		}

		#region Events
		void OnEncounter(EncounterRequest request)
		{
			SetOnEncoutnerLast();

			switch (request.State)
			{
				case EncounterRequest.States.Request:
					OnBegin(
						request.GameModel,
						request.Encounter
					);
					break;
				case EncounterRequest.States.Next:
					if (nextLogDelay.HasValue) nextLogDelay = 0f;
					break;
				case EncounterRequest.States.Done:
					callbacks.EncounterRequest(EncounterRequest.Complete());
					break;
				case EncounterRequest.States.Complete:
					encounterService.GetEncounterInteraction(configuration.Encounter.EncounterId).TimesCompleted.Value++;
					configuration.Model.EncounterState.SetEncounterStatus(EncounterStatus.Completed(configuration.Encounter.EncounterId));

					configuration.Model.EncounterState.State.Value = EncounterStateModel.States.Ending;

					OnEnd();
					break;
			}
		}

		void OnStateChange(StateChange change)
		{
			if (configuration.Model == null || configuration.Model.EncounterState.State.Value == EncounterStateModel.States.Complete || !change.Is(StateMachine.States.Game, StateMachine.Events.End)) return;
			OnEnd();
		}

		void OnUpdate(float delta)
		{
			if (!nextLogDelay.HasValue) return;

			if (currentPreferences().EncounterLogsAutoNext.Value) nextLogDelay = Mathf.Max(0f, nextLogDelay.Value - delta);

			if (!Mathf.Approximately(0f, nextLogDelay.Value)) return;

			OnNextLog(nextLog);
		}

		void OnBegin(
			GameModel model,
			EncounterInfoModel encounter
		)
		{
			if (model.EncounterState.State.Value != EncounterStateModel.States.Complete)
			{
				Debug.LogError("Beginning an encounter while one is not complete, may cause unpredictable behaviour.");
			}

			model.EncounterState.State.Value = EncounterStateModel.States.Processing;

			configuration.Model = model;
			configuration.Encounter = encounter;

			model.EncounterState.RegisterKeyValueListener(keyValueService);

			callbacks.SaveRequest(SaveRequest.Request(OnBeginSaved));
		}

		void OnBeginSaved(SaveRequest request)
		{
			if (request.Status != RequestStatus.Success)
			{
				Debug.LogError("Beginning an encounter without successfully saving first may cause unpredictable behaviour.");
			}

			configuration.Model.SaveState.Value = SaveStateBlock.NotSavable(Strings.CannotSaveReasons.CurrentlyInEncounter);

			nextLog = configuration.Encounter.Logs.Beginning;
			if (nextLog == null)
			{
				Debug.LogError("No beginning found for encounter " + configuration.Encounter.EncounterId.Value);

				callbacks.EncounterRequest(EncounterRequest.Controls(false, true));
				return;
			}
			nextLogDelay = 0f;
		}

		void OnEnd()
		{
			configuration.Model.SaveState.Value = SaveStateBlock.Savable();

			var oldModel = configuration.Model;

			configuration.Model = null;
			configuration.Encounter = null;

			nextLog = null;
			nextLogDelay = null;

			oldModel.EncounterState.UnRegisterKeyValueListener(); // This used to be below the other one, but I think that was wrong...
			oldModel.EncounterState.State.Value = EncounterStateModel.States.Complete;
		}

		void OnNextLog(EncounterLogModel logModel)
		{
			nextLog = null;
			nextLogDelay = null;

			// Some logic may be halting, so we have a done action for them to
			// call when they're finished.
			Action linearDone = () => OnHandledLog(logModel, logModel.NextLog);
			Action<string> nonLinearDone = nextLog => OnHandledLog(logModel, nextLog);

			// TODO: Should probably send out request event that it responds to by itself...
			var wasHandled = false;

			foreach (var handler in handlers)
			{
				if (handler.Handle(logModel, linearDone, nonLinearDone))
				{
					wasHandled = true;
					break;
				}
			}

			if (!wasHandled)
			{
				Debug.LogError("Unrecognized LogType: " + logModel.LogType + ", skipping...");
				linearDone();
			}
		}

		void OnHandledLog(EncounterLogModel logModel, string nextLogId)
		{
			if (logModel.Ending.Value)
			{
				callbacks.EncounterRequest(EncounterRequest.Controls(false, true));
				return;
			}

			if (string.IsNullOrEmpty(nextLogId))
			{
				Debug.Log("Handle null next logs here!");
				return;
			}
			nextLog = configuration.Encounter.Logs.GetLogFirstOrDefault(nextLogId);
			if (nextLog == null)
			{
				Debug.LogError("Next log could not be found.");
				return;
			}
			callbacks.EncounterRequest(EncounterRequest.Controls(true, false));
			nextLogDelay = logModel.TotalDuration;
		}
		#endregion
	}
}