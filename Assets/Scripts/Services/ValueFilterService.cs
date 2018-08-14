using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class ValueFilterService
	{
		CallbackService callbacks;
		ILogService logger;

		public ValueFilterService(CallbackService callbacks, ILogService logger)
		{
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (logger == null) throw new ArgumentNullException("logger");

			this.callbacks = callbacks;
			this.logger = logger;
		}

		public void Filter(Action<bool> done, ValueFilterModel filter, GameModel model)
		{
			var remaining = filter.Filters.Value.Where(f => !f.FilterIgnore).ToList();

			bool? anyResult = null;
			bool? allResult = null;
			bool? noneResult = null;

			OnFilter(done, anyResult, allResult, noneResult, remaining, model);
		}

		void OnFilter(
			Action<bool> done,
			bool? anyResult,
			bool? allResult,
			bool? noneResult,
			List<IValueFilterEntryModel> remaining,
			GameModel model
		)
		{
			Debug.Log("any: " + anyResult + " all: " + allResult + " none: " + noneResult);
			if (remaining.Count == 0)
			{
				var passed = true;
				passed &= !anyResult.HasValue || anyResult.Value;
				passed &= !allResult.HasValue || allResult.Value;
				passed &= !noneResult.HasValue || noneResult.Value;
				done(passed);
				return;
			}
			var current = remaining.First();
			remaining.RemoveAt(0);

			Action<ValueFilterGroups, bool> filterDone = (group, result) =>
			{
				OnFilterResult(group, result, current.FilterNegate, done, anyResult, allResult, noneResult, remaining, model);
			};

			switch (current.FilterType)
			{
				case ValueFilterTypes.KeyValueBoolean:
					OnHandle(current as BooleanKeyValueFilterEntryModel, filterDone);
					break;
				case ValueFilterTypes.EncounterInteraction:
					OnHandle(current as EncounterInteractionFilterEntryModel, model, filterDone);
					break;
				default:
					Debug.LogError("Unrecognized FilterType: " + current.FilterType + ", skipping...");
					OnFilter(done, anyResult, allResult, noneResult, remaining, model);
					break;
			}
		}

		void OnFilterResult(
			ValueFilterGroups group,
			bool result,
			bool negated,
			Action<bool> done,
			bool? anyResult,
			bool? allResult,
			bool? noneResult,
			List<IValueFilterEntryModel> remaining,
			GameModel model
		)
		{
			result = negated ? !result : result;

			Debug.Log(result);

			switch (group)
			{
				case ValueFilterGroups.Any:
					anyResult = anyResult.HasValue ? (anyResult.Value || result) : result;
					break;
				case ValueFilterGroups.All:
					allResult = allResult.HasValue ? (allResult.Value && result) : result;
					break;
				case ValueFilterGroups.None:
					noneResult = noneResult.HasValue ? (noneResult.Value && !result) : !result;
					break;
				default:
					Debug.LogError("Unrecognized group: " + group + ", skipping...");
					break;
			}
			OnFilter(done, anyResult, allResult, noneResult, remaining, model);
		}

		#region Handling
		void OnHandle(BooleanKeyValueFilterEntryModel filter, Action<ValueFilterGroups, bool> done)
		{
			callbacks.KeyValueRequest(
				KeyValueRequest.Get(
					filter.Target.Value,
					filter.Key.Value,
					// If the boolean KV is equal to the result on the filter, the result is true.
					result => done(filter.Group.Value, result.Value == filter.FilterValue.Value)
				)
			);
		}

		void OnHandle(EncounterInteractionFilterEntryModel filter, GameModel model, Action<ValueFilterGroups, bool> done)
		{
			var operation = filter.Operation.Value;
			var encounterInteraction = model.GetEncounterStatus(filter.FilterValue.Value);
			var result = false;

			if (operation == EncounterInteractionFilterOperations.NotCompleted)
			{
				result = encounterInteraction.State != EncounterStatus.States.Completed;
			}
			else
			{
				switch (encounterInteraction.State)
				{
					case EncounterStatus.States.Unknown:
					case EncounterStatus.States.NeverSeen:
						result = operation == EncounterInteractionFilterOperations.NeverSeen;
						break;
					case EncounterStatus.States.Seen:
						result = operation == EncounterInteractionFilterOperations.Seen;
						break;
					case EncounterStatus.States.Completed:
						result = operation == EncounterInteractionFilterOperations.Completed;
						break;
					default:
						Debug.LogError("Unrecognized EncounterInteraction: " + encounterInteraction.State);
						break;
				}
			}

			done(filter.Group.Value, result);
		}
		#endregion
	}
}