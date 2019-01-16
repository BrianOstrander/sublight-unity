﻿using System;
using System.Linq;
using System.Collections.Generic;

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
		IUniverseService universeService;
		Func<PreferencesModel> currentPreferences;

		GameModel model;
		EncounterInfoModel encounter;

		EncounterLogModel nextLog;
		float? nextLogDelay;

		public EncounterHandlerService(
			Heartbeat heartbeat,
			CallbackService callbacks,
			EncounterService encounterService,
			KeyValueService keyValueService,
			ValueFilterService valueFilter,
			IUniverseService universeService,
			Func<PreferencesModel> currentPreferences
		)
		{
			if (heartbeat == null) throw new ArgumentNullException("heartbeat");
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (encounterService == null) throw new ArgumentNullException("encounterService");
			if (keyValueService == null) throw new ArgumentNullException("keyValueService");
			if (valueFilter == null) throw new ArgumentNullException("valueFilter");
			if (universeService == null) throw new ArgumentNullException("universeService");
			if (currentPreferences == null) throw new ArgumentNullException("currentPreferences");

			this.heartbeat = heartbeat;
			this.callbacks = callbacks;
			this.encounterService = encounterService;
			this.keyValueService = keyValueService;
			this.valueFilter = valueFilter;
			this.universeService = universeService;
			this.currentPreferences = currentPreferences;

			this.callbacks.EncounterRequest += OnEncounter;
			this.callbacks.StateChange += OnStateChange;
			this.heartbeat.Update += OnUpdate;
		}

		#region Events
		void OnEncounter(EncounterRequest request)
		{
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
					encounterService.GetEncounterInteraction(encounter.EncounterId).TimesCompleted.Value++;
					model.EncounterState.SetEncounterStatus(EncounterStatus.Completed(encounter.EncounterId));

					model.EncounterState.State.Value = EncounterStateModel.States.Ending;

					Debug.LogWarning("TODO: Logic upon completing encounter!");
					//callbacks.FocusRequest(
					//	new SystemsFocusRequest(
					//		toFocus.SystemZero,
					//		toFocus
					//	)
					//);
					break;
			}
		}

		/*
		 * TODO: Update this to use the new focus system.
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Encounter:
					// We only begin the encounter once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;

					nextLog = encounter.Logs.Beginning;
					if (nextLog == null)
					{
						Debug.LogError("No beginning found for encounter " + encounter.EncounterId.Value);

						callbacks.EncounterRequest(EncounterRequest.Controls(false, true));
						break;
					}
					nextLogDelay = 0f;
					break;
				default:
					if (state != States.Ending) break;
					// We only save once we've completely moved to the next focus.
					if (focus.State != FocusRequest.States.Complete) return;
					model.SaveState.Value = SaveStateBlock.Savable();
					OnEnd();
					callbacks.SaveRequest(SaveRequest.Request());
					break;
			}
		}
		*/

		void OnStateChange(StateChange change)
		{
			if (model == null || model.EncounterState.State.Value == EncounterStateModel.States.Complete || !change.Is(StateMachine.States.Game, StateMachine.Events.End)) return;
			OnEnd();
		}

		void OnUpdate(float delta)
		{
			if (!nextLogDelay.HasValue) return;
			if (callbacks.LastPlayState.State != PlayState.States.Playing) return;

			if (currentPreferences().EncounterLogsAutoNext.Value) nextLogDelay = Mathf.Max(0f, nextLogDelay.Value - delta);

			if (!Mathf.Approximately(0f, nextLogDelay.Value)) return;

			OnShowLog(nextLog);
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

			this.model = model;
			this.encounter = encounter;

			model.EncounterState.RegisterKeyValueListener(keyValueService);

			callbacks.SaveRequest(SaveRequest.Request(OnBeginSaved));
		}

		void OnBeginSaved(SaveRequest request)
		{
			if (request.Status != RequestStatus.Success)
			{
				Debug.LogError("Beginning an encounter without successfully saving first may cause unpredictable behaviour.");
			}

			model.SaveState.Value = SaveStateBlock.NotSavable(Strings.CannotSaveReasons.CurrentlyInEncounter);

			nextLog = encounter.Logs.Beginning;
			if (nextLog == null)
			{
				Debug.LogError("No beginning found for encounter " + encounter.EncounterId.Value);

				callbacks.EncounterRequest(EncounterRequest.Controls(false, true));
				return;
			}
			nextLogDelay = 0f;
		}

		void OnEnd()
		{
			var oldModel = model;

			model = null;
			encounter = null;

			nextLog = null;
			nextLogDelay = null;

			oldModel.EncounterState.State.Value = EncounterStateModel.States.Complete;
			oldModel.EncounterState.UnRegisterKeyValueListener();
		}

		void OnShowLog(EncounterLogModel logModel)
		{
			nextLog = null;
			nextLogDelay = null;

			// Some logic may be halting, so we have a done action for them to
			// call when they're finished.
			Action linearDone = () => OnHandledLog(logModel, logModel.NextLog);
			Action<string> nonLinearDone = nextLog => OnHandledLog(logModel, nextLog);

			// TODO: Should probably send out request event that it responds to by itself...

			switch (logModel.LogType)
			{
				case EncounterLogTypes.Text:
					OnTextLog(logModel as TextEncounterLogModel, linearDone);
					break;
				case EncounterLogTypes.KeyValue:
					OnKeyValueLog(logModel as KeyValueEncounterLogModel, linearDone);
					break;
				case EncounterLogTypes.Switch:
					OnSwitchLog(logModel as SwitchEncounterLogModel, nonLinearDone);
					break;
				case EncounterLogTypes.Button:
					OnButtonLog(logModel as ButtonEncounterLogModel, nonLinearDone);
					break;
				case EncounterLogTypes.Encyclopedia:
					OnEncyclopediaLog(logModel as EncyclopediaEncounterLogModel, linearDone);
					break;
				case EncounterLogTypes.Event:
					OnEncounterEventLog(logModel as EncounterEventEncounterLogModel, linearDone);
					break;
				default:
					Debug.LogError("Unrecognized LogType: " + logModel.LogType + ", skipping...");
					linearDone();
					break;
			}
		}

		void OnTextLog(TextEncounterLogModel logModel, Action done)
		{
			var result = new TextHandlerModel();
			result.Log.Value = logModel;
			result.Message.Value = logModel.Message;

			callbacks.EncounterRequest(EncounterRequest.Handle(result));

			done();
		}

		void OnKeyValueLog(KeyValueEncounterLogModel logModel, Action done)
		{
			var total = logModel.Operations.Value.Length;

			if (total == 0)
			{
				done();
				return;
			}

			var progress = 0;

			foreach (var entry in logModel.Operations.Value)
			{
				switch (entry.Operation)
				{
					case KeyValueOperations.SetString:
						var setString = entry as SetStringOperationModel;
						callbacks.KeyValueRequest(
							KeyValueRequest.Set(
								entry.Target.Value,
								entry.Key.Value,
								setString.Value.Value,
								result => OnKeyValueLogDone(result, total, ref progress, done)
							)
						);
						break;
					case KeyValueOperations.SetBoolean:
						var setBoolean = entry as SetBooleanOperationModel;
						callbacks.KeyValueRequest(
							KeyValueRequest.Set(
								entry.Target.Value,
								entry.Key.Value,
								setBoolean.Value.Value,
								result => OnKeyValueLogDone(result, total, ref progress, done)
							)
						);
						break;
					default:
						Debug.LogError("Unrecognized KeyValueType: " + entry.Operation);
						done();
						return;
				}
			}
		}

		void OnKeyValueLogDone<T>(KeyValueResult<T> result, int total, ref int progress, Action done) where T : IConvertible
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Setting " + result.TargetKey + " = " + result.Value + " returned with status: " + result.Status + " and error:\n" + result.Error);
				Debug.LogWarning("Continuing after this failure may result in unpredictable behaviour.");
			}
			progress++;
			if (total == progress) done();
		}

		void OnSwitchLog(SwitchEncounterLogModel logModel, Action<string> done)
		{
			var switches = logModel.Switches.Value.Where(e => !e.Ignore.Value && !string.IsNullOrEmpty(e.NextLogId.Value)).OrderBy(e => e.Index.Value).ToList();

			OnSwitchLogFilter(
				null,
				null,
				switches,
				(status, result) => OnSwitchLogDone(status, result, logModel, done)
			);
		}

		void OnSwitchLogFilter(
			bool? result,
			string resultId,
			List<SwitchEdgeModel> remaining,
			Action<RequestStatus, string> done
		)
		{
			if (result.HasValue && result.Value)
			{
				done(RequestStatus.Success, resultId);
				return;
			}

			if (!remaining.Any())
			{
				done(RequestStatus.Failure, null);
				return;
			}

			var next = remaining.First();
			var nextId = next.NextLogId.Value;
			remaining.RemoveAt(0);

			valueFilter.Filter(
				filterResult => OnSwitchLogFilter(filterResult, nextId, remaining, done),
				next.Filtering,
				model,
				encounter
			);
		}

		void OnSwitchLogDone(RequestStatus status, string nextLogId, SwitchEncounterLogModel logModel, Action<string> done)
		{
			if (status == RequestStatus.Success) done(nextLogId);
			else done(logModel.NextLog);
		}

		void OnButtonLog(ButtonEncounterLogModel logModel, Action<string> done)
		{
			var buttons = logModel.Buttons.Value.Where(e => !e.Ignore.Value && !string.IsNullOrEmpty(e.NextLogId.Value)).OrderBy(e => e.Index.Value).ToList();

			Action<RequestStatus, List<ButtonLogBlock>> filteringDone = (status, filtered) => OnButtonLogDone(status, filtered, logModel, done);

			OnButtonLogFilter(
				null,
				buttons,
				new List<ButtonLogBlock>(),
				done,
				filteringDone
			);
		}

		void OnButtonLogFilter(
			ButtonLogBlock? result,
			List<ButtonEdgeModel> remaining,
			List<ButtonLogBlock> filtered,
			Action<string> done,
			Action<RequestStatus, List<ButtonLogBlock>> filteringDone
		)
		{
			if (result.HasValue) filtered.Add(result.Value);

			if (remaining.None())
			{
				// No remaining to filter.
				if (filtered.Where(f => f.Interactable).Any()) filteringDone(RequestStatus.Success, filtered); // There are interactable buttons.
				else filteringDone(RequestStatus.Failure, null); // There are no interactable buttons.
				return;
			}

			Action<ButtonLogBlock?> nextDone = filterResult => OnButtonLogFilter(filterResult, remaining, filtered, done, filteringDone);
			var next = remaining.First();
			remaining.RemoveAt(0);
			var possibleResult = new ButtonLogBlock(
				next.Message.Value,
				false,
				true,
				() => OnButtonLogClick(next, () => done(next.NextLogId.Value))
			);

			if (next.AutoDisableEnabled.Value)
			{
				// When this button is pressed, it gets disabled, so we have to check.
				callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						next.AutoDisabledKey,
						kvResult => OnButtonLogAutoDisabled(kvResult, next, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto disabled check.
				OnButtonLogAutoDisabled(
					null,
					next,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogAutoDisabled(
			KeyValueResult<bool>? kvResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled check, so see the result.
				if (kvResult.Value.Value)
				{
					// It has been auto disabled.
					nextDone(null);
					return;
				}
			}

			if (edge.AutoDisableInteractions.Value)
			{
				// When this button is pressed, interactions get disabled, so we have to check.
				callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						edge.AutoDisabledInteractionsKey,
						interactionKvResult => OnButtonLogAutoDisabledInteractions(interactionKvResult, edge, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto disable interactions check.
				OnButtonLogAutoDisabledInteractions(
					null,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogAutoDisabledInteractions(
			KeyValueResult<bool>? kvResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled interaction check, so set the result.
				possibleResult.Interactable = !kvResult.Value.Value;
			}

			if (!edge.NotAutoUsed.Value)
			{
				// When this button is pressed, it gets marked as used, so we have to check to see if that happened.
				callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						edge.AutoUsedKey,
						autoUsedKvResult => OnButtonLogAutoUsed(autoUsedKvResult, edge, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto used check.
				OnButtonLogAutoUsed(
					null,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogAutoUsed(
			KeyValueResult<bool>? kvResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled interaction check, so set the result.
				possibleResult.Used = kvResult.Value.Value;
			}

			valueFilter.Filter(
				filterResult => OnButtonLogEnabledFiltering(filterResult, edge, possibleResult, nextDone),
				edge.EnabledFiltering,
				model,
				encounter
			);
		}

		void OnButtonLogEnabledFiltering(
			bool filteringResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (!filteringResult)
			{
				// This button isn't enabled.
				nextDone(null);
				return;
			}

			if (possibleResult.Interactable)
			{
				// It hasn't automatically been disabled, so we check the filter.
				valueFilter.Filter(
					filterResult => OnButtonLogInteractableFiltering(filterResult, edge, possibleResult, nextDone),
					edge.InteractableFiltering,
					model,
					encounter
				);
			}
			else
			{
				// Already not interactable, so bypass the filter.
				OnButtonLogInteractableFiltering(
					false,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogInteractableFiltering(
			bool filteringResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			possibleResult.Interactable &= filteringResult;

			if (!possibleResult.Used)
			{
				// Hasn't been auto marked as used, so we have to check the filter.
				valueFilter.Filter(
					filterResult => OnButtonLogUsedFiltering(filterResult, edge, possibleResult, nextDone),
					edge.UsedFiltering,
					model,
					encounter
				);
			}
			else
			{
				// Auto using made it used, so we bypass the filter.
				OnButtonLogUsedFiltering(
					true,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogUsedFiltering(
			bool filteringResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			possibleResult.Used |= filteringResult;

			nextDone(possibleResult);
		}

		void OnButtonLogDone(RequestStatus status, List<ButtonLogBlock> buttons, ButtonEncounterLogModel logModel, Action<string> done)
		{
			if (status != RequestStatus.Success)
			{
				// No enabled and interactable buttons found.
				done(logModel.NextLog);
				return;
			}

			var result = new ButtonHandlerModel();
			result.Log.Value = logModel;
			result.Buttons.Value = buttons.ToArray();

			callbacks.EncounterRequest(EncounterRequest.Handle(result));
		}

		void OnButtonLogClick(ButtonEdgeModel edge, Action done)
		{
			if (!edge.NotAutoUsed)
			{
				// We need to set this to be used.
				callbacks.KeyValueRequest(
					KeyValueRequest.Set(
						KeyValueTargets.Encounter,
						edge.AutoUsedKey,
						true,
						result => OnButtonLogClickAutoUse(edge, done)
					)
				);
			}
			else
			{
				// Bypass setting it to be used.
				OnButtonLogClickAutoUse(edge, done);
			}
		}

		void OnButtonLogClickAutoUse(ButtonEdgeModel edge, Action done)
		{
			if (edge.AutoDisableInteractions)
			{
				// We need to disable future interactions with this.
				callbacks.KeyValueRequest(
					KeyValueRequest.Set(
						KeyValueTargets.Encounter,
						edge.AutoDisabledInteractionsKey,
						true,
						result => OnButtonLogClickAutoDisableInteractions(edge, done)
					)
				);
			}
			else
			{
				// Bypass setting future interactions.
				OnButtonLogClickAutoDisableInteractions(edge, done);
			}
		}

		void OnButtonLogClickAutoDisableInteractions(ButtonEdgeModel edge, Action done)
		{
			if (edge.AutoDisableEnabled)
			{
				// We need to disable this button for future interactions.
				callbacks.KeyValueRequest(
					KeyValueRequest.Set(
						KeyValueTargets.Encounter,
						edge.AutoDisabledKey,
						true,
						result => OnButtonLogClickAutoDisableEnabled(edge, done)
					)
				);
			}
			else
			{
				// Bypass disabling this button.
				OnButtonLogClickAutoDisableEnabled(edge, done);
			}
		}

		void OnButtonLogClickAutoDisableEnabled(ButtonEdgeModel edge, Action done)
		{
			done();
		}

		void OnEncyclopediaLog(EncyclopediaEncounterLogModel logModel, Action done)
		{
			model.Encyclopedia.Add(logModel.Entries.Value.Select(e => e.Entry.Duplicate).ToArray());

			done();
		}

		void OnEncounterEventLog(EncounterEventEncounterLogModel logModel, Action done)
		{
			var events = logModel.Edges.Where(e => !e.Ignore.Value).OrderBy(e => e.Index.Value).Select(e => e.Entry).ToList();

			Action<RequestStatus, List<EncounterEventEntryModel>> filteringDone = (status, filtered) => OnEncounterEventLogDone(status, filtered, logModel, done);

			OnEncounterEventLogFilter(
				null,
				events,
				new List<EncounterEventEntryModel>(),
				filteringDone
			);
		}

		void OnEncounterEventLogFilter(
			EncounterEventEntryModel result,
			List<EncounterEventEntryModel> remaining,
			List<EncounterEventEntryModel> filtered,
			Action<RequestStatus, List<EncounterEventEntryModel>> filteringDone
		)
		{
			if (result != null) filtered.Add(result);

			if (remaining.None())
			{
				// No remaining to filter.
				if (filtered.Any()) filteringDone(RequestStatus.Success, filtered); // There are callable events.
				else filteringDone(RequestStatus.Failure, null); // There are no callable events.
				return;
			}

			Action<EncounterEventEntryModel> nextDone = filterResult => OnEncounterEventLogFilter(filterResult, remaining, filtered, filteringDone);
			var next = remaining.First();
			remaining.RemoveAt(0);

			valueFilter.Filter(
				filterResult => OnEncounterEventLogFiltering(filterResult, next, nextDone),
				next.Filtering,
				model,
				encounter
			);
		}

		void OnEncounterEventLogFiltering(
			bool filteringResult,
			EncounterEventEntryModel possibleResult,
			Action<EncounterEventEntryModel> nextDone
		)
		{
			if (filteringResult) nextDone(possibleResult);
			else nextDone(null);
		}

		void OnEncounterEventLogDone(
			RequestStatus status,
			List<EncounterEventEntryModel> events,
			EncounterEventEncounterLogModel logModel,
			Action done
		)
		{
			if (status != RequestStatus.Success)
			{
				// No enabled and callable events found.
				done();
				return;
			}

			var result = new EncounterEventHandlerModel();
			result.Log.Value = logModel;
			result.Events.Value = events.ToArray();
			result.IsHalting.Value = logModel.IsHalting.Value || events.Any(e => e.IsHalting.Value);

			if (result.IsHalting.Value) result.HaltingDone.Value = done;

			callbacks.EncounterRequest(EncounterRequest.Handle(result));

			if (!result.IsHalting.Value) done();
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
			nextLog = encounter.Logs.GetLogFirstOrDefault(nextLogId);
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