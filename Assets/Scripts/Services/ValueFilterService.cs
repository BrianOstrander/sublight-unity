using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Configuration;
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
				case ValueFilterTypes.KeyValueEnumeration:
					OnHandle(current as EnumerationKeyValueFilterEntryModel, filterDone);
					break;
				case ValueFilterTypes.EncounterInteraction:
					OnHandle(current as EncounterInteractionFilterEntryModel, model, encounterModel, filterDone);
					break;
				case ValueFilterTypes.ModuleTrait:
					OnHandle(current as ModuleTraitFilterEntryModel, model, filterDone);
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
			// The input may or may not be local, so we check.
			switch (filter.Input1.Source)
			{
				case KeyValueSources.LocalValue:
					OnHandle(filter, filter.Input1.LocalValue, done);
					break;
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.Get(
							filter.Input1.ForeignTarget,
							filter.Input1.ForeignKey,
							result => OnHandle(filter, result.Value, done)
						)
					);
					break;
				default:
					Debug.LogError("Unrecognized Input.Source: " + filter.Input1.Source);
					break;
			}
		}

		void OnHandle(BooleanKeyValueFilterEntryModel filter, bool inputValue, Action<ValueFilterGroups, bool> done)
		{
			// The operand should always be foreign.
			switch (filter.Input0.Source)
			{
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.Get(
							filter.Input0.ForeignTarget,
							filter.Input0.ForeignKey,
							// If the boolean KV is equal to the result on the filter, the result is true.
							result => done(filter.Group.Value, result.Value == inputValue)
						)
					);
					break;
				default:
					Debug.LogError("Local operands not supported");
					break;
			}
		}

		void OnHandle(IntegerKeyValueFilterEntryModel filter, Action<ValueFilterGroups, bool> done)
		{
			// The input may or may not be local, so we check.
			switch (filter.Input1.Source)
			{
				case KeyValueSources.LocalValue:
					OnHandle(filter, filter.Input1.LocalValue, done);
					break;
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.GetInteger(
							filter.Input1.ForeignTarget,
							filter.Input1.ForeignKey,
							result => OnHandle(filter, result.Value, done)
						)
					);
					break;
				default:
					Debug.LogError("Unrecognized Input.Source: " + filter.Input1.Source);
					break;
			}
		}

		void OnHandle(IntegerKeyValueFilterEntryModel filter, int inputValue, Action<ValueFilterGroups, bool> done)
		{
			Action<KeyValueResult<int>> onGet = result =>
			{
				var passed = false;
				switch (filter.Operation.Value)
				{
					case IntegerFilterOperations.Equals:
						passed = result.Value == inputValue;
						break;
					case IntegerFilterOperations.NotEquals:
						passed = result.Value != inputValue;
						break;
					case IntegerFilterOperations.LessThanOrEquals:
						passed = result.Value <= inputValue;
						break;
					case IntegerFilterOperations.GreaterThanOrEquals:
						passed = result.Value >= inputValue;
						break;
					case IntegerFilterOperations.LessThan:
						passed = result.Value < inputValue;
						break;
					case IntegerFilterOperations.GreaterThan:
						passed = result.Value > inputValue;
						break;
					default:
						Debug.LogError("Unrecognized Operation: " + filter.Operation.Value);
						break;
				}
				done(filter.Group.Value, passed);
			};

			// The operand should always be foreign
			switch (filter.Input0.Source)
			{
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.Get(
							filter.Input0.ForeignTarget,
							filter.Input0.ForeignKey,
							onGet
						)
					);
					break;
				default:
					Debug.LogError("Local operands not supported");
					break;
			}
		}

		void OnHandle(StringKeyValueFilterEntryModel filter, Action<ValueFilterGroups, bool> done)
		{
			// The input may or may not be local, so we check.
			switch (filter.Input1.Source)
			{
				case KeyValueSources.LocalValue:
					OnHandle(filter, filter.Input1.LocalValue, done);
					break;
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.Get(
							filter.Input1.ForeignTarget,
							filter.Input1.ForeignKey,
							result => OnHandle(filter, result.Value, done)
						)
					);
					break;
				default:
					Debug.LogError("Unrecognized Input.Source: " + filter.Input1.Source);
					break;
			}
		}

		void OnHandle(StringKeyValueFilterEntryModel filter, string inputValue, Action<ValueFilterGroups, bool> done)
		{
			// The operand should always be foreign.
			switch (filter.Input0.Source)
			{
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.Get(
							filter.Input0.ForeignTarget,
							filter.Input0.ForeignKey,
							// If the string KV is equal to the result on the filter, the result is true.
							result =>
							{
								var passed = false;
								switch (filter.Operation.Value)
								{
									case StringFilterOperations.Equals:
										passed = result.Value == inputValue;
										break;
									case StringFilterOperations.NormalizedEquals:
										if (string.IsNullOrEmpty(result.Value) == string.IsNullOrEmpty(inputValue))
										{
											if (string.IsNullOrEmpty(inputValue)) passed = true;
											else passed = result.Value.ToLower() == inputValue.ToLower();
										}
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
					break;
				default:
					Debug.LogError("Local operands are not supported");
					break;
			}
		}

		void OnHandle(FloatKeyValueFilterEntryModel filter, Action<ValueFilterGroups, bool> done)
		{
			// The input may or may not be local, so we check.
			switch (filter.Input1.Source)
			{
				case KeyValueSources.LocalValue:
					OnHandle(filter, filter.Input1.LocalValue, done);
					break;
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.GetFloat(
							filter.Input1.ForeignTarget,
							filter.Input1.ForeignKey,
							result => OnHandle(filter, result.Value, done)
						)
					);
					break;
				default:
					Debug.LogError("Unrecognized Input.Source: " + filter.Input1.Source);
					break;
			}
		}

		void OnHandle(FloatKeyValueFilterEntryModel filter, float floatValue, Action<ValueFilterGroups, bool> done)
		{
			Action<KeyValueResult<float>> onGet = result =>
			{
				var passed = false;
				switch (filter.Operation.Value)
				{
					case FloatFilterOperations.Equals:
						passed = Mathf.Approximately(result.Value, floatValue);
						break;
					case FloatFilterOperations.NotEquals:
						passed = !Mathf.Approximately(result.Value, floatValue);
						break;
					case FloatFilterOperations.LessThanOrEquals:
						passed = result.Value < floatValue || Mathf.Approximately(result.Value, floatValue);
						break;
					case FloatFilterOperations.GreaterThanOrEquals:
						passed = result.Value > floatValue || Mathf.Approximately(result.Value, floatValue);
						break;
					case FloatFilterOperations.LessThan:
						passed = result.Value < floatValue;
						break;
					case FloatFilterOperations.GreaterThan:
						passed = result.Value > floatValue;
						break;
					default:
						Debug.LogError("Unrecognized Operation: " + filter.Operation.Value);
						break;
				}
				done(filter.Group.Value, passed);
			};

			// Operand must be foreign.
			switch (filter.Input0.Source)
			{
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.Get(
							filter.Input0.ForeignTarget,
							filter.Input0.ForeignKey,
							onGet
						)
					);
					break;
				default:
					Debug.LogError("Local operands are not supported");
					break;
			}
		}

		void OnHandle(EnumerationKeyValueFilterEntryModel filter, Action<ValueFilterGroups, bool> done)
		{
			// The input may or may not be local, so we check.
			switch (filter.Input1.Source)
			{
				case KeyValueSources.LocalValue:
					OnHandle(filter, filter.Input1.LocalValue, done);
					break;
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.Get(
							filter.Input1.ForeignTarget,
							filter.Input1.ForeignKey,
							result => OnHandle(filter, result.Value, done)
						)
					);
					break;
				default:
					Debug.LogError("Unrecognized Input.Source: " + filter.Input1.Source);
					break;
			}
		}

		void OnHandle(EnumerationKeyValueFilterEntryModel filter, int inputValue, Action<ValueFilterGroups, bool> done)
		{
			// The operand should always be foreign.
			switch (filter.Input0.Source)
			{
				case KeyValueSources.KeyValue:
					callbacks.KeyValueRequest(
						KeyValueRequest.GetInteger(
							filter.Input0.ForeignTarget,
							filter.Input0.ForeignKey,
							// If the string KV is equal to the result on the filter, the result is true.
							result =>
							{
								var passed = false;
								switch (filter.Operation.Value)
								{
									case EnumerationFilterOperations.Equals:
										passed = result.Value == inputValue;
										break;
									case EnumerationFilterOperations.NotEquals:
										passed = result.Value != inputValue;
										break;
									default:
										Debug.LogError("Unrecognized Operation: " + filter.Operation.Value);
										break;
								}
								done(filter.Group.Value, passed);
							}
						)
					);
					break;
				default:
					Debug.LogError("Local operands are not supported");
					break;
			}
		}

		void OnHandle(
			EncounterInteractionFilterEntryModel filter,
			GameModel model,
			EncounterInfoModel encounterModel,
			Action<ValueFilterGroups, bool> done
		)
		{
			var encounterId = string.IsNullOrEmpty(filter.FilterValue.Value) ? (encounterModel == null ? null : encounterModel.Id.Value) : filter.FilterValue.Value;
			var operation = filter.Operation.Value;
			var encounterInteraction = model.EncounterStatuses.GetEncounterStatus(encounterId);
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
			ModuleTraitFilterEntryModel filter,
			GameModel model,
			Action<ValueFilterGroups, bool> done
		)
		{
			var result = false;

			switch (filter.Operation.Value)
			{
				case ModuleTraitFilterOperations.Present:
					// model.Context.ModuleService.
					var modules = model.Ship.Modules.Value;
					if (filter.ValidModuleTypes.Value.Any()) modules = modules.Where(m => filter.ValidModuleTypes.Value.Contains(m.Type.Value)).ToArray();
					result = modules.Any(m => m.TraitIds.Value.Contains(filter.FilterValue.Value));
					break;
				case ModuleTraitFilterOperations.Appendable:
					OnHandleModuleTraitCanAdd(
						model,
						filter,
						model.Ship.Modules.Value.ToList(),
						done
					);
					return;
				default:
					Debug.LogError("Unrecognized " + nameof(ModuleTraitFilterOperations) + ": " + filter.Operation.Value);
					break;
			}
			
			done(filter.Group.Value, result);
		}

		void OnHandleModuleTraitCanAdd(
			GameModel model,
			ModuleTraitFilterEntryModel filter,
			List<ModuleModel> remaining,
			Action<ValueFilterGroups, bool> done
		)
		{
			if (remaining.None())
			{
				done(filter.Group.Value, false);
				return;
			}
			
			var module = remaining.First();
			remaining.RemoveAt(0);
			
			model.Context.ModuleService.CanAppendTraits(
				module,
				canAppendResult =>
				{
					switch (canAppendResult.Status)
					{
						case RequestStatus.Success:
							if (canAppendResult.Payload.Valid.Any()) done(filter.Group.Value, true);
							else
							{
								OnHandleModuleTraitCanAdd(
									model,
									filter,
									remaining,
									done
								);
							}
							break;
						default:
							canAppendResult.Log();
							break;
					}
				},
				filter.FilterValue.Value
			);
		}
		#endregion
	}
}