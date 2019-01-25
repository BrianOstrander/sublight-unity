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
							default:
								Debug.LogError("Unrecognized Encounter EventType " + entry.EncounterEvent.Value + ". May cause halting issues.");
								break;
						}
					}
				};
				Action onHaltingDone = () =>
				{
					if (handler.AlwaysHaltingOrHasHaltingEvents && handler.HaltingDone.Value != null) handler.HaltingDone.Value();
				};

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

				payload.Game.ToolbarSelectionRequest.Value = ToolbarSelectionRequest.Create(
					targetSelection,
					targetLocking == EncounterEvents.ToolbarSelection.LockStates.Lock,
					ToolbarSelectionRequest.Sources.Encounter,
					done
				);
			}
		}
	}
}