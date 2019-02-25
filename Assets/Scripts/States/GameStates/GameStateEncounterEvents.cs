using System;
using System.Linq;
using System.Collections.Generic;

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
				GamePayload payload,
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
							case EncounterEvents.Types.Debug:
								OnHandleEventDebugLog(payload, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.ToolbarSelection:
								OnHandleEventToolbarSelection(payload, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.DumpKeyValues:
								OnHandleEventDumpKeyValues(payload, entry, currOnEventDone);
								break;
							case EncounterEvents.Types.PopTriggers:
								OnHandleEventPopTriggers(payload, entry, currOnEventDone);
								break;
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

			static void OnHandleEventDebugLog(
				GamePayload payload,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var severity = entry.KeyValues.GetEnum(EncounterEvents.Debug.EnumKeys.Severity, EncounterEvents.Debug.Severities.Error);
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
				GamePayload payload,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var currentSelection = payload.Game.ToolbarSelection.Value;
				var targetSelection = entry.KeyValues.GetEnum(EncounterEvents.ToolbarSelection.EnumKeys.Selection, currentSelection);

				if (targetSelection == ToolbarSelections.Unknown) targetSelection = currentSelection;

				var currentLocking = payload.Game.ToolbarLocking.Value ? EncounterEvents.ToolbarSelection.LockStates.Lock : EncounterEvents.ToolbarSelection.LockStates.UnLock;
				var targetLocking = entry.KeyValues.GetEnum(EncounterEvents.ToolbarSelection.EnumKeys.LockState, currentLocking);

				if (targetLocking == EncounterEvents.ToolbarSelection.LockStates.Unknown) targetLocking = currentLocking;

				if (currentSelection == targetSelection && currentLocking == targetLocking)
				{
					done();
					return;
				}

				payload.Game.Context.ToolbarSelectionRequest.Value = ToolbarSelectionRequest.Create(
					targetSelection,
					targetLocking == EncounterEvents.ToolbarSelection.LockStates.Lock,
					ToolbarSelectionRequest.Sources.Encounter,
					done
				);
			}

			static void OnHandleEventDumpKeyValues(
				GamePayload payload,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var dumpTarget = entry.KeyValues.GetEnum(EncounterEvents.DumpKeyValues.EnumKeys.Target, KeyValueTargets.Unknown);

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
							result += OnHandleEventDumpKeyValuesInstance("Encounter", payload.Game.Context.EncounterState.KeyValues);
							break;
						case KeyValueTargets.Game:
							result += OnHandleEventDumpKeyValuesInstance("Game", payload.Game.KeyValues);
							break;
						case KeyValueTargets.Global:
							result += OnHandleEventDumpKeyValuesInstance("Global", App.MetaKeyValues.GlobalKeyValues);
							break;
						case KeyValueTargets.Preferences:
							result += OnHandleEventDumpKeyValuesInstance("Preferences", App.MetaKeyValues.PreferencesKeyValues);
							break;
						case KeyValueTargets.CelestialSystem:
							result += OnHandleEventDumpKeyValuesInstance("CelestialSystem", payload.Game.Context.CurrentSystem.Value == null ? null : payload.Game.Context.CurrentSystem.Value.KeyValues);
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
				GamePayload payload,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var popped = new List<EncounterTriggers>();

				if (entry.KeyValues.GetBoolean(EncounterEvents.PopTriggers.BooleanKeys.PopTransitComplete)) popped.Add(EncounterTriggers.TransitComplete);
				if (entry.KeyValues.GetBoolean(EncounterEvents.PopTriggers.BooleanKeys.PopResourceRequest)) popped.Add(EncounterTriggers.ResourceRequest);
				if (entry.KeyValues.GetBoolean(EncounterEvents.PopTriggers.BooleanKeys.PopResourceConsume)) popped.Add(EncounterTriggers.ResourceConsume);
				if (entry.KeyValues.GetBoolean(EncounterEvents.PopTriggers.BooleanKeys.PopSystemIdle)) popped.Add(EncounterTriggers.SystemIdle);

				Debug.Log(popped.Count());

				payload.Game.EncounterTriggers.Value = payload.Game.EncounterTriggers.Value.Where(t => !popped.Contains(t)).ToArray();

				done();
			}
		}
	}
}