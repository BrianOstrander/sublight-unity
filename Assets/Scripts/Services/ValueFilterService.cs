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

		public ValueFilterService(CallbackService callbacks)
		{
			if (callbacks == null) throw new ArgumentNullException("callbacks");

			this.callbacks = callbacks;
		}

		public void Filter(
			Action<bool> done,
			ValueFilterModel filter,
			GameModel model,
			EncounterInfoModel encounterModel
		)
		{
			var remaining = filter.Filters.Value.Where(f => !f.FilterIgnore).ToList();

			if (remaining.None())
			{
				done(!filter.FalseByDefault.Value);
				return;
			}

			bool? anyResult = null;
			bool? allResult = null;
			bool? noneResult = null;

			OnFilter(
				done,
				anyResult,
				allResult,
				noneResult,
				remaining,
				model,
				encounterModel
			);
		}

		void OnFilter(
			Action<bool> done,
			bool? anyResult,
			bool? allResult,
			bool? noneResult,
			List<IValueFilterEntryModel> remaining,
			GameModel model,
			EncounterInfoModel encounterModel
		)
		{
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
				OnFilterResult(
					group,
					result,
					current.FilterNegate,
					done,
					anyResult,
					allResult,
					noneResult,
					remaining,
					model,
					encounterModel
				);
			};

			switch (current.FilterType)
			{
				case ValueFilterTypes.KeyValueBoolean:
					OnHandle(current as BooleanKeyValueFilterEntryModel, filterDone);
					break;
				case ValueFilterTypes.KeyValueInteger:
					OnHandle(current as IntegerKeyValueFilterEntryModel, filterDone);
					break;
				case ValueFilterTypes.KeyValueString:
					OnHandle(current as StringKeyValueFilterEntryModel, filterDone);
					break;
				case ValueFilterTypes.KeyValueFloat:
					OnHandle(current as FloatKeyValueFilterEntryModel, filterDone);
					break;
				case ValueFilterTypes.EncounterInteraction:
					OnHandle(current as EncounterInteractionFilterEntryModel, model, encounterModel, filterDone);
					break;
				default:
					Debug.LogError("Unrecognized FilterType: " + current.FilterType + ", skipping...");
					OnFilter(done, anyResult, allResult, noneResult, remaining, model, encounterModel);
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
			GameModel model,
			EncounterInfoModel encounterModel
		)
		{
			result = negated ? !result : result;

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
			OnFilter(
				done,
				anyResult,
				allResult,
				noneResult,
				remaining,
				model,
				encounterModel
			);
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

		void OnHandle(IntegerKeyValueFilterEntryModel filter, Action<ValueFilterGroups, bool> done)
		{
			Action<KeyValueResult<int>> onGet = result =>
			{
				var passed = false;
				switch (filter.Operation.Value)
				{
					case IntegerFilterOperations.Equals:
						passed = result.Value == filter.FilterValue.Value;
						break;
					case IntegerFilterOperations.NotEquals:
						passed = result.Value != filter.FilterValue.Value;
						break;
					case IntegerFilterOperations.LessThanOrEquals:
						passed = result.Value <= filter.FilterValue.Value;
						break;
					case IntegerFilterOperations.GreaterThanOrEquals:
						passed = result.Value >= filter.FilterValue.Value;
						break;
					case IntegerFilterOperations.LessThan:
						passed = result.Value < filter.FilterValue.Value;
						break;
					case IntegerFilterOperations.GreaterThan:
						passed = result.Value > filter.FilterValue.Value;
						break;
					default:
						Debug.LogError("Unrecognized Operation: " + filter.Operation.Value);
						break;
				}
				done(filter.Group.Value, passed);
			};

			callbacks.KeyValueRequest(
				KeyValueRequest.Get(
					filter.Target.Value,
					filter.Key.Value,
					onGet
				)
			);
		}

		void OnHandle(StringKeyValueFilterEntryModel filter, Action<ValueFilterGroups, bool> done)
		{
			callbacks.KeyValueRequest(
				KeyValueRequest.Get(
					filter.Target.Value,
					filter.Key.Value,
					// If the string KV is equal to the result on the filter, the result is true.
					result =>
					{
						var passed = false;
						switch (filter.Operation.Value)
						{
							case StringFilterOperations.Equals:
								passed = result.Value == filter.FilterValue.Value;
								break;
							case StringFilterOperations.IsNullOrEmpty:
								passed = string.IsNullOrEmpty(result.Value);
								break;
							case StringFilterOperations.IsNull:
								passed = result.Value == null;
								break;
							default:
								Debug.LogError("Unrecognized Operation: " + filter.Operation.Value);
								break;
						}
						done(filter.Group.Value, passed);
					}
				)
			);
		}

		void OnHandle(FloatKeyValueFilterEntryModel filter, Action<ValueFilterGroups, bool> done)
		{
			Action<KeyValueResult<float>> onGet = result =>
			{
				var passed = false;
				switch (filter.Operation.Value)
				{
					case FloatFilterOperations.Equals:
						passed = Mathf.Approximately(result.Value, filter.FilterValue.Value);
						break;
					case FloatFilterOperations.NotEquals:
						passed = !Mathf.Approximately(result.Value, filter.FilterValue.Value);
						break;
					case FloatFilterOperations.LessThanOrEquals:
						passed = result.Value < filter.FilterValue.Value || Mathf.Approximately(result.Value, filter.FilterValue.Value);
						break;
					case FloatFilterOperations.GreaterThanOrEquals:
						passed = result.Value > filter.FilterValue.Value || Mathf.Approximately(result.Value, filter.FilterValue.Value);
						break;
					case FloatFilterOperations.LessThan:
						passed = result.Value < filter.FilterValue.Value;
						break;
					case FloatFilterOperations.GreaterThan:
						passed = result.Value > filter.FilterValue.Value;
						break;
					default:
						Debug.LogError("Unrecognized Operation: " + filter.Operation.Value);
						break;
				}
				done(filter.Group.Value, passed);
			};

			callbacks.KeyValueRequest(
				KeyValueRequest.Get(
					filter.Target.Value,
					filter.Key.Value,
					onGet
				)
			);
		}

		void OnHandle(
			EncounterInteractionFilterEntryModel filter,
			GameModel model,
			EncounterInfoModel encounterModel,
			Action<ValueFilterGroups, bool> done
		)
		{
			var encounterId = string.IsNullOrEmpty(filter.FilterValue.Value) ? (encounterModel == null ? null : encounterModel.EncounterId.Value) : filter.FilterValue.Value;
			var operation = filter.Operation.Value;
			var encounterInteraction = model.Context.EncounterState.GetEncounterStatus(encounterId);
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