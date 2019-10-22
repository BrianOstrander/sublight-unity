using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class GameState
	{
		// Kinda weird but whatever...
		public class Encounter
		{
			public static void OnHandleEvent(
				GameState state,
				EncounterEventHandlerModel handler
			)
			{
				var remainingHaltingEvents = new List<string>();

				if (handler.AlwaysHalting.Value)
				{
					foreach (var entry in handler.Events.Value) remainingHaltingEvents.Add(entry.EntryId.Value);
				}
				else if (handler.HasHaltingEvents.Value)
				{
					foreach (var entry in handler.Events.Value.Where(e => e.IsHalting.Value)) remainingHaltingEvents.Add(entry.EntryId.Value);
				}

				Func<bool> onHaltingCondition = () => remainingHaltingEvents.Count == 0;
				Action<string> onEventDone = eventId => remainingHaltingEvents.Remove(eventId);
				Action onCallEvents = () =>
				{
					foreach (var entry in handler.Events.Value)
					{
						var currEventId = entry.EntryId.Value; // Not sure if this is necessary, can't remember if the bug was fixed.
						Action currOnEventDone = () => onEventDone(currEventId);

						switch (entry.EncounterEvent.Value)
						{
							case EncounterEvents.Types.Custom:
								OnHandleEventCustom(state, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.Debug:
								OnHandleEventDebugLog(state, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.ToolbarSelection:
								OnHandleEventToolbarSelection(state, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.DumpKeyValues:
								OnHandleEventDumpKeyValues(state, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.TriggerQueue:
								OnHandleEventPopTriggers(state, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.Delay:
								OnHandleEventDelay(state, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.AudioSnapshot:
								OnHandleEventAudioSnapshot(state, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.Waypoint:
								OnHandleEventWaypoint(state, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.ModuleTrait:
								OnHandleEventModuleTrait(state, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.RefreshSystem:
							case EncounterEvents.Types.GameComplete:
								// Some presenter takes care of this.
								currOnEventDone();
								break;
							default:
								Debug.LogError("Unrecognized Encounter EventType " + entry.EncounterEvent.Value + ". May cause halting issues, will try to invoke callback.");
								currOnEventDone();
								break;
						}
					}
				};
				Action onHaltingDone = handler.HaltingDone.Value;

				App.SM.PushBlocking<GameState>(onCallEvents, onHaltingCondition, "CallEncounterEvents");
				App.SM.Push<GameState>(onHaltingDone, "CallEncounterEventsDone");
			}

			static void OnHandleEventCustom(
				GameState state,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var request = new EncounterEventsCustomRequest(
					entry.KeyValues,
					entry.IsHalting ? done : null
				);

				Debug.Log("Handling custom event: " + request.EventName);

				App.Callbacks.EncounterEventsCustom(request);

				if (!entry.IsHalting) done();
			}

			static void OnHandleEventDebugLog(
				GameState state,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var severity = entry.KeyValues.GetEnumeration(EncounterEvents.Debug.EnumKeys.Severity, EncounterEvents.Debug.Severities.Error);
				var message = entry.KeyValues.GetString(EncounterEvents.Debug.StringKeys.Message);
				if (string.IsNullOrEmpty(message)) message = "< no message was provided >";

				switch (severity)
				{
					case EncounterEvents.Debug.Severities.Unknown:
					case EncounterEvents.Debug.Severities.Normal:
						Debug.Log(message);
						break;
					case EncounterEvents.Debug.Severities.Warning:
						Debug.LogWarning(message);
						break;
					case EncounterEvents.Debug.Severities.Error:
						Debug.LogError(message);
						break;
					case EncounterEvents.Debug.Severities.Break:
						if (Application.isEditor)
						{
							Debug.LogWarning("Encounter Break: " + message);
							Debug.Break();
						}
						else
						{
							Debug.LogError("Encounter tried to call a break outside the editor with this message: " + message);
						}
						break;
					default:
						Debug.LogError("Unrecognized severity: " + severity);
						break;
				}

				done();
			}

			static void OnHandleEventToolbarSelection(
				GameState state,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var currentSelection = state.Payload.Game.ToolbarSelection.Value;
				var targetSelection = entry.KeyValues.GetEnumeration(EncounterEvents.ToolbarSelection.EnumKeys.Selection, currentSelection);

				if (targetSelection == ToolbarSelections.Unknown) targetSelection = currentSelection;

				var currentLocking = state.Payload.Game.ToolbarLocking.Value ? EncounterEvents.ToolbarSelection.LockStates.Lock : EncounterEvents.ToolbarSelection.LockStates.UnLock;
				var targetLocking = entry.KeyValues.GetEnumeration(EncounterEvents.ToolbarSelection.EnumKeys.LockState, currentLocking);

				if (targetLocking == EncounterEvents.ToolbarSelection.LockStates.Unknown) targetLocking = currentLocking;

				if (currentSelection == targetSelection && currentLocking == targetLocking)
				{
					done();
					return;
				}

				// We're already waiting for this event to be done... so we can't push anything to the state machine.
				// This could be fixed by not wrapping all events in one big block up above.
				App.Heartbeat.Wait(
					() => 
					{
						state.Payload.Game.Context.ToolbarSelectionRequest.Value = ToolbarSelectionRequest.Create(
							targetSelection,
							targetLocking == EncounterEvents.ToolbarSelection.LockStates.Lock,
							ToolbarSelectionRequest.Sources.Encounter,
							done
						);
					},
					() => App.Callbacks.LastTransitionFocusRequest.State == TransitionFocusRequest.States.Complete
				);
			}

			static void OnHandleEventDumpKeyValues(
				GameState state,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var dumpTarget = entry.KeyValues.GetEnumeration(EncounterEvents.DumpKeyValues.EnumKeys.Target, KeyValueTargets.Unknown);

				var values = EnumExtensions.GetValues(KeyValueTargets.Unknown);

				switch(dumpTarget)
				{
					case KeyValueTargets.Unknown: break;
					default: values = new KeyValueTargets[] { dumpTarget }; break;
				}

				var result = string.Empty;

				foreach (var target in values)
				{
					switch (target)
					{
						case KeyValueTargets.Encounter:
							result += OnHandleEventDumpKeyValuesInstance("Encounter", state.Payload.Game.Context.EncounterState.KeyValues);
							break;
						case KeyValueTargets.Game:
							result += OnHandleEventDumpKeyValuesInstance("Game", state.Payload.Game.KeyValues);
							break;
						case KeyValueTargets.Global:
							result += OnHandleEventDumpKeyValuesInstance("Global", App.MetaKeyValues.GlobalKeyValues);
							break;
						case KeyValueTargets.Preferences:
							result += OnHandleEventDumpKeyValuesInstance("Preferences", App.MetaKeyValues.PreferencesKeyValues);
							break;
						case KeyValueTargets.CelestialSystem:
							result += OnHandleEventDumpKeyValuesInstance("CelestialSystem", state.Payload.Game.Context.CurrentSystem.Value == null ? null : state.Payload.Game.Context.CurrentSystem.Value.KeyValues);
							break;
						default:
							Debug.LogError("Unrecognized Target: " + target);
							break;
					}
					result += "\n---------------\n";
				}

				Debug.Log("Dumping Key Values...\n" + result);

				done();
			}

			static string OnHandleEventDumpKeyValuesInstance(string name, KeyValueListModel keyValues)
			{
				if (keyValues == null) return name + " is null\n";
				return keyValues.Dump(name);
			}

			static void OnHandleEventPopTriggers(
				GameState state,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var popped = new List<EncounterTriggers>();
				var pushed = new Dictionary<EncounterTriggers, int>();

				foreach (var trigger in EnumExtensions.GetValues(EncounterTriggers.Unknown))
				{
					if (entry.KeyValues.GetBoolean(EncounterEvents.TriggerQueue.BooleanKeys.PopTrigger(trigger))) popped.Add(trigger);

					var pushIndex = entry.KeyValues.GetInteger(
						EncounterEvents.TriggerQueue.IntegerKeys.PushTrigger(trigger),
						EncounterEvents.TriggerQueue.PushDisabled
					);
					if (pushIndex != EncounterEvents.TriggerQueue.PushDisabled) pushed.Add(trigger, pushIndex);
				}

				var result = state.Payload.Game.EncounterTriggers.Value.Where(t => !popped.Contains(t)).ToList();

				foreach (var push in pushed.OrderByDescending(p => p.Value).Select(p => p.Key)) result.Add(push);

				state.Payload.Game.EncounterTriggers.Value = result.ToArray();

				done();
			}
		}

		static void OnHandleEventDelay(
			GameState state,
			EncounterEventEntryModel entry,
			Action done
		)
		{
			var trigger = entry.KeyValues.GetEnumeration<EncounterEvents.Delay.Triggers>(EncounterEvents.Delay.EnumKeys.Trigger);

			switch (trigger)
			{
				case EncounterEvents.Delay.Triggers.Time:
					var timeDuration = entry.KeyValues.GetFloat(EncounterEvents.Delay.FloatKeys.TimeDuration);
					if (float.IsNaN(timeDuration))
					{
						Debug.LogError("Specified Time Duration was NaN");
						done();
						return;
					}

					App.Heartbeat.Wait(
						done,
						Mathf.Max(0f, timeDuration)
					);
					break;
				default:
					Debug.LogError("Unrecognized Trigger: " + trigger);
					break;
			}
		}

		static void OnHandleEventAudioSnapshot(
			GameState state,
			EncounterEventEntryModel entry,
			Action done
		)
		{
			var snapshotName = entry.KeyValues.GetString(EncounterEvents.AudioSnapshot.StringKeys.SnapshotName);
			var transitionDuration = Mathf.Max(0f, entry.KeyValues.GetFloat(EncounterEvents.AudioSnapshot.FloatKeys.TransitionDuration));

			if (string.IsNullOrEmpty(snapshotName))
			{
				Debug.LogError("Encounter event specified a null or empty AudioSnapshot.SnapshotName");
				done();
				return;
			}

			App.Audio.SetSnapshot(snapshotName, transitionDuration);

			App.Heartbeat.Wait(
				done,
				transitionDuration
			);
		}

		static void OnHandleEventWaypoint(
			GameState state,
			EncounterEventEntryModel entry,
			Action done
		)
		{
			var waypointId = entry.KeyValues.GetString(EncounterEvents.Waypoint.StringKeys.WaypointId);
			var visibility = entry.KeyValues.GetEnumeration<WaypointModel.VisibilityStates>(EncounterEvents.Waypoint.EnumKeys.Visibility);

			if (string.IsNullOrEmpty(waypointId))
			{
				Debug.LogError("A waypoint encounter event specified a null or empty WaypointId, this is not valid. Attempting to skip...");
				done();
				return;
			}

			var waypoint = state.Payload.Game.Waypoints.GetWaypointFirstOrDefault(waypointId);

			if (waypoint == null)
			{
				// Threw a warning here just in case... might not be needed...
				Debug.LogWarning("Unable to find waypoint with id \"" + waypointId + "\", this may result in unpredictable behaviour. Attempting to skip...");
				done();
				return;
			}

			switch (visibility)
			{
				case WaypointModel.VisibilityStates.Unknown: break;
				default:
					waypoint.VisibilityState.Value = visibility;
					break;
			}

			done();
		}
		
		static void OnHandleEventModuleTrait(
			GameState state,
			EncounterEventEntryModel entry,
			Action done
		)
		{
			var operationId = entry.KeyValues.GetString(EncounterEvents.ModuleTrait.StringKeys.OperationId);
			var operation = entry.KeyValues.GetEnumeration<EncounterEvents.ModuleTrait.Operations>(EncounterEvents.ModuleTrait.EnumKeys.Operation);

			var moduleTypes = new List<ModuleTypes>();
			var allModuleTypes = EnumExtensions.GetValues(ModuleTypes.Unknown);
			
			foreach (var moduleType in allModuleTypes)
			{
				if (entry.KeyValues.GetBoolean(EncounterEvents.ModuleTrait.BooleanKeys.ModuleTypeIsValid(moduleType))) moduleTypes.Add(moduleType);
			}

			if (moduleTypes.None()) moduleTypes = allModuleTypes.ToList();

			var modules = state.Payload.Game.Ship.Modules.Value.Where(m => moduleTypes.Contains(m.Type.Value)).ToList();
			
			foreach (var module in state.Payload.Game.Ship.Modules.Value.Where(m => moduleTypes.Contains(m.Type.Value )))
			{
				switch (operation)
				{
					case EncounterEvents.ModuleTrait.Operations.AppendByTraitId:
						OnHandleEventModuleTraitAppendByTraitIdNext(
							state,
							modules,
							operationId,
							done
						);
						break;
					case EncounterEvents.ModuleTrait.Operations.RemoveByTraitId:
						OnHandleEventModuleTraitRemoveByTraitIdNext(
							state,
							modules,
							operationId,
							done
						);
						break;
					case EncounterEvents.ModuleTrait.Operations.RemoveByFamilyId:
						OnHandleEventModuleTraitRemoveByFamilyIdNext(
							state,
							modules,
							operationId,
							done
						);
						break;
					default:
						Debug.LogError("Unrecognized " + EncounterEvents.ModuleTrait.EnumKeys.Operation + ": " + operation);
						break;
				}
			}
		}

		static void OnHandleEventModuleTraitAppendByTraitIdNext(
			GameState state,
			List<ModuleModel> remaining,
			string traitId,
			Action done
		)
		{
			if (remaining.None())
			{
				OnHandleEventModuleTraitOperationDone(state, done);
				return;
			}

			var next = remaining.First();
			remaining.RemoveAt(0);
			
			state.Payload.Game.Context.ModuleService.AppendTraits(
				next,
				result =>
				{
					switch (result.Status)
					{
						case RequestStatus.Success:
							if (result.Payload.Appended.Any())
							{
								// TODO: Add to audit log here...	
							}
							OnHandleEventModuleTraitAppendByTraitIdNext(
								state,
								remaining,
								traitId,
								done
							);
							break;
						default:
							result.Log("Appending trait by encounter event failed");
							done();
							break;
					}
				},
				traitId
			);
		}
		
		static void OnHandleEventModuleTraitRemoveByTraitIdNext(
			GameState state,
			List<ModuleModel> remaining,
			string traitId,
			Action done
		)
		{
			foreach (var module in remaining)
			{
				var traitIdsRemoved = module.TraitIds.Value.Where(t => t == traitId); 
				if (traitIdsRemoved.None()) continue;
				
				// TODO: Add to audit log here...
				module.TraitIds.Value = module.TraitIds.Value.Except(traitIdsRemoved).ToArray();
			}
			OnHandleEventModuleTraitOperationDone(state, done);
		}
		
		static void OnHandleEventModuleTraitRemoveByFamilyIdNext(
			GameState state,
			List<ModuleModel> remaining,
			string familyId,
			Action done
		)
		{
			var traitIdsInFamily = state.Payload.Game.Context.ModuleService.GetTraitsByFamilyId(familyId).Select(t => t.Id.Value);
			foreach (var module in remaining)
			{
				var traitIdsRemoved = module.TraitIds.Value.Where(t => traitIdsInFamily.Contains(t));
				if (traitIdsRemoved.None()) continue;
				
				// TODO: Add to audit log here...
				module.TraitIds.Value = module.TraitIds.Value.Except(traitIdsRemoved).ToArray();
			}
			OnHandleEventModuleTraitOperationDone(state, done);
		}

		static void OnHandleEventModuleTraitOperationDone(
			GameState state,
			Action done
		)
		{
			Debug.LogWarning("TODO: Append to audit log here!");
			state.Payload.Game.Ship.Modules.Value = state.Payload.Game.Ship.Modules.Value;
			done();
		}
	}
}