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

		public void Filter(Action<bool> done, ValueFilterModel filter, GameModel model)
		{
			Filter(done, filter, model, null);
		}

		public void Filter(Action<bool> done, ValueFilterModel filter, GameModel model, InventoryModel inventoryModel)
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

			OnFilter(done, anyResult, allResult, noneResult, remaining, model, inventoryModel);
		}

		void OnFilter(
			Action<bool> done,
			bool? anyResult,
			bool? allResult,
			bool? noneResult,
			List<IValueFilterEntryModel> remaining,
			GameModel model,
			InventoryModel inventoryModel
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
				OnFilterResult(group, result, current.FilterNegate, done, anyResult, allResult, noneResult, remaining, model, inventoryModel);
			};

			switch (current.FilterType)
			{
				case ValueFilterTypes.KeyValueBoolean:
					OnHandle(current as BooleanKeyValueFilterEntryModel, filterDone);
					break;
				case ValueFilterTypes.KeyValueString:
					OnHandle(current as StringKeyValueFilterEntryModel, filterDone);
					break;
				case ValueFilterTypes.EncounterInteraction:
					OnHandle(current as EncounterInteractionFilterEntryModel, model, filterDone);
					break;
				case ValueFilterTypes.InventoryId:
					OnHandle(current as IdInventoryFilterEntryModel, model, inventoryModel, filterDone);
					break;
				case ValueFilterTypes.InventoryTag:
					OnHandle(current as TagInventoryFilterEntryModel, model, inventoryModel, filterDone);
					break;
				default:
					Debug.LogError("Unrecognized FilterType: " + current.FilterType + ", skipping...");
					OnFilter(done, anyResult, allResult, noneResult, remaining, model, inventoryModel);
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
			InventoryModel inventoryModel
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
			OnFilter(done, anyResult, allResult, noneResult, remaining, model, inventoryModel);
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

		void OnHandle(
			IdInventoryFilterEntryModel filter,
			GameModel model,
			InventoryModel inventoryModel,
			Action<ValueFilterGroups, bool> done)
		{
			var result = false;

			switch (filter.InventoryFilterType.Value)
			{
				case InventoryFilterTypes.Inventory:
					if (inventoryModel != null) Debug.LogError("InventoryFilterType is " + filter.InventoryFilterType.Value + " but an inventoryModel was provided.");
					result = model.Ship.Value.Inventory.HasInventory(filter.FilterValue.Value);
					break;
				case InventoryFilterTypes.References:
					if (inventoryModel == null) Debug.LogError("InventoryFilterType is " + filter.InventoryFilterType.Value + " but no inventoryModel was provided.");
					else result = inventoryModel.InventoryId.Value == filter.FilterValue.Value;
					break;
				default:
					Debug.LogError("Unrecognized InventoryFilterType: " + filter.InventoryFilterType.Value);
					break;
			}

			done(filter.Group.Value, result);
		}

		void OnHandle(
			TagInventoryFilterEntryModel filter,
			GameModel model,
			InventoryModel inventoryModel,
			Action<ValueFilterGroups, bool> done)
		{
			var result = false;
			
			var smoothed = filter.FilterValue.Value;
			if (string.IsNullOrEmpty(smoothed))
			{
				Debug.LogError("FilterValue was null or empty.");
				done(filter.Group.Value, result);
				return;
			}
			smoothed = smoothed.ToLower();

			switch (filter.InventoryFilterType.Value)
			{
				case InventoryFilterTypes.Inventory:
					if (inventoryModel != null) Debug.LogError("InventoryFilterType is " + filter.InventoryFilterType.Value + " but an inventoryModel was provided.");
					result = model.Ship.Value.Inventory.HasInventory(i => i.Tags.Value.Any(t => t.ToLower() == smoothed));
					break;
				case InventoryFilterTypes.References:
					if (inventoryModel == null) Debug.LogError("InventoryFilterType is " + filter.InventoryFilterType.Value + " but no inventoryModel was provided.");
					else result = inventoryModel.Tags.Value.Any(t => t.ToLower() == smoothed);
					break;
				default:
					Debug.LogError("Unrecognized InventoryFilterType: " + filter.InventoryFilterType.Value);
					break;
			}

			done(filter.Group.Value, result);
		}
		#endregion
	}
}