using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;
using LunraGames.SubLight.Presenters;

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
					foreach (var entry in handler.Events.Value) remainingHaltingEvents.Add(entry.EventId.Value);
				}
				else if (handler.HasHaltingEvents.Value)
				{
					foreach (var entry in handler.Events.Value.Where(e => e.IsHalting.Value)) remainingHaltingEvents.Add(entry.EventId.Value);
				}

				Func<bool> onHaltingCondition = () => remainingHaltingEvents.Count == 0;
				Action<string> onEventDone = eventId => remainingHaltingEvents.Remove(eventId);
				Action onCallEvents = () =>
				{
					foreach (var entry in handler.Events.Value)
					{
						var currEventId = entry.EventId.Value; // Not sure if this is necessary, can't remember if the bug was fixed.
						Action currOnEventDone = () => onEventDone(currEventId);

						switch (entry.EncounterEvent.Value)
						{
							case EncounterEvents.Types.DebugLog:
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

				App.SM.PushBlocking(onCallEvents, onHaltingCondition);
				App.SM.Push(onHaltingDone);
			}

			static void OnHandleEventDebugLog(
				GamePayload payload,
				EncounterEventEntryModel entry,
				Action done
			)
			{
				var severity = entry.KeyValues.GetEnum(EncounterEvents.DebugLog.EnumKeys.Severity, EncounterEvents.DebugLog.Severities.Error);
				var message = entry.KeyValues.GetString(EncounterEvents.DebugLog.StringKeys.Message);
				if (string.IsNullOrEmpty(message)) message = "< no message was provided >";

				switch (severity)
				{
					case EncounterEvents.DebugLog.Severities.Normal:
						Debug.Log(message);
						break;
					case EncounterEvents.DebugLog.Severities.Warning:
						Debug.LogWarning(message);
						break;
					case EncounterEvents.DebugLog.Severities.Error:
						Debug.LogError(message);
						break;
					case EncounterEvents.DebugLog.Severities.Break:
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
				var targetSelection = entry.KeyValues.GetEnum(EncounterEvents.ToolbarSelection.EnumKeys.Selection, payload.Game.ToolbarSelection.Value);
				if (payload.Game.ToolbarSelection.Value == targetSelection)
				{
					done();
					return;
				}

				payload.Game.ToolbarSelectionRequest.Value = ToolbarSelectionRequest.Create(targetSelection, done);
			}
		}
	}
}