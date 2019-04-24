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

		StateMachineWrapper sm;

		DialogLanguageBlock saveDisabledDuringEncounterLanguage;

		#region Dynamic During Encounter
		EncounterLogModel nextLog;
		float? nextLogDelay;
		Action popSaveBlocker;
		#endregion

		EncounterLogHandlerConfiguration configuration;
		IEncounterLogHandler[] handlers;

		public EncounterHandlerService(
			Heartbeat heartbeat,
			CallbackService callbacks,
			EncounterService encounterService,
			KeyValueService keyValueService,
			ValueFilterService valueFilter,
			Func<PreferencesModel> currentPreferences,
			StateMachine stateMachine,
			DialogLanguageBlock saveDisabledDuringEncounterLanguage
		)
		{
			if (heartbeat == null) throw new ArgumentNullException("heartbeat");
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (encounterService == null) throw new ArgumentNullException("encounterService");
			if (keyValueService == null) throw new ArgumentNullException("keyValueService");
			if (valueFilter == null) throw new ArgumentNullException("valueFilter");
			if (currentPreferences == null) throw new ArgumentNullException("currentPreferences");
			if (stateMachine == null) throw new ArgumentNullException("stateMachine");

			sm = new StateMachineWrapper(stateMachine, GetType());

			this.heartbeat = heartbeat;
			this.callbacks = callbacks;
			this.encounterService = encounterService;
			this.keyValueService = keyValueService;
			this.valueFilter = valueFilter;
			this.currentPreferences = currentPreferences;

			this.saveDisabledDuringEncounterLanguage = saveDisabledDuringEncounterLanguage;

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
				new KeyValueLogHandler(configuration),
				new SwitchLogHandler(configuration),
				new SwitchLogHandler(configuration),
				new ButtonLogHandler(configuration),
				new EncyclopediaLogHandler(configuration),
				new EncounterEventLogHandler(configuration),
				new DialogLogHandler(configuration),
				new BustLogHandler(configuration),
				new ConversationLogHandler(configuration)
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
				case EncounterRequest.States.PrepareComplete:
					sm.Push(
						() => callbacks.EncounterRequest(EncounterRequest.Complete()),
						"CallComplete"
					);
					break;
				case EncounterRequest.States.Complete:
					encounterService.GetEncounterInteraction(configuration.Encounter.Id).TimesCompleted.Value++;
					configuration.Model.EncounterStatuses.SetEncounterStatus(EncounterStatus.Completed(configuration.Encounter.Id));

					configuration.Model.Context.EncounterState.Current.Value = configuration.Model.Context.EncounterState.Current.Value.NewState(EncounterStateModel.States.Ending);

					callbacks.SaveRequest(SaveRequest.Request(OnEndSaved));
					break;
			}
		}

		void OnStateChange(StateChange change)
		{
			if (configuration.Model == null || configuration.Model.Context.EncounterState.Current.Value.State == EncounterStateModel.States.Complete || !change.Is(StateMachine.States.Game, StateMachine.Events.End)) return;
			OnEnd();
		}

		void OnUpdate(float delta)
		{
			if (!nextLogDelay.HasValue) return;

			if (!Mathf.Approximately(0f, nextLogDelay.Value)) return;

			OnNextLog(nextLog);
		}

		void OnBegin(
			GameModel model,
			EncounterInfoModel encounter
		)
		{
			if (model == null) throw new ArgumentNullException("model");
			if (encounter == null) throw new ArgumentNullException("encounter");

			if (model.Context.EncounterState.Current.Value.State == EncounterStateModel.States.Unknown)
			{
				Debug.LogError("Encounter state is currently Unknown, may cause unpredictable behaviour.");
			}
			else if (model.Context.EncounterState.Current.Value.State != EncounterStateModel.States.Complete)
			{
				Debug.LogError("Beginning an encounter while one is not complete, may cause unpredictable behaviour.");
			}

			App.Analytics.EncounterBegin(model, encounter);

			model.Context.EncounterState.Current.Value = new EncounterStateModel.Details(EncounterStateModel.States.Processing, encounter.Id.Value);

			configuration.Model = model;
			configuration.Encounter = encounter;

			model.Context.EncounterState.RegisterKeyValueListener(keyValueService);

			callbacks.SaveRequest(SaveRequest.Request(OnBeginSaved));
		}

		void OnBeginSaved(SaveRequest request)
		{
			if (request.Status != RequestStatus.Success)
			{
				Debug.LogError("Beginning an encounter without successfully saving first may cause unpredictable behaviour.");
			}

			popSaveBlocker = configuration.Model.Context.SaveBlockers.Push(saveDisabledDuringEncounterLanguage);

			nextLog = configuration.Encounter.Logs.Beginning;
			if (nextLog == null)
			{
				Debug.LogError("No beginning found for encounter " + configuration.Encounter.Id.Value);

				callbacks.EncounterRequest(EncounterRequest.Controls(false, true));
				return;
			}
			nextLogDelay = 0f;
		}

		void OnEndSaved(SaveRequest request)
		{
			if (request.Status != RequestStatus.Success)
			{
				Debug.LogError("Failed to save upon ending encounter.");
			}

			OnEnd();
		}

		void OnEnd()
		{
			App.Analytics.EncounterEnd(configuration.Encounter);

			popSaveBlocker();

			var oldModel = configuration.Model;

			configuration.Model = null;
			configuration.Encounter = null;

			nextLog = null;
			nextLogDelay = null;

			oldModel.Context.EncounterState.UnRegisterKeyValueListener(); // This used to be below the other one, but I think that was wrong...
			oldModel.Context.EncounterState.Current.Value = oldModel.Context.EncounterState.Current.Value.NewState(EncounterStateModel.States.Complete);
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