using System;
using System.Linq;

using UnityEditor;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGamesEditor;
using DefinedState = LunraGames.SubLight.KeyDefinitionEditorWindow.DefinedState;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutValueFilter
	{
		struct KeyValueCreateInfo
		{
			public KeyValueTargets ForeignTarget;
			public string ForeignKey;
		}

		const float OperatorWidth = 120f;
		const float KeyValueKeyWidth = 170f;

		static string changedFilterId;

		public static void Field(string name, ValueFilterModel model, Color? color = null)
		{
			Field(new GUIContent(name), model, color);
		}

		public static void Field(GUIContent content, ValueFilterModel model, Color? color = null)
		{
			EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, color, color.HasValue);
			{
				GUILayout.BeginHorizontal();
				{
					var noFilters = !model.HasFilters;
					if (noFilters) EditorGUILayoutExtensions.PushColor(Color.gray);
					{
						EditorGUIExtensions.PauseChangeCheck();
						{
							model.ShowValues.Value = EditorGUILayout.Foldout(model.ShowValues.Value, content);
						}
						EditorGUIExtensions.UnPauseChangeCheck();
					}
					if (noFilters) EditorGUILayoutExtensions.PopColor();

					if (model.ShowValues.Value)
					{
						EditorGUILayoutExtensions.PushColor(Color.gray);
						{
							GUILayout.Label("Hold 'alt' to delete filters.", GUILayout.ExpandWidth(false));
						}
						EditorGUILayoutExtensions.PopColor();
					}

					GUILayout.Label("Default", GUILayout.ExpandWidth(false));
					model.FalseByDefault.Value = !EditorGUILayoutExtensions.ToggleButtonValue(!model.FalseByDefault.Value, style: EditorStyles.miniButton);
				}
				GUILayout.EndHorizontal();

				if (model.ShowValues.Value) Values(model);
			}
			EditorGUILayoutExtensions.EndVertical();
		}

		public static void Values(ValueFilterModel model)
		{
			var filters = model.Filters.Value;
			var filterCount = filters.Length;
			var any = filters.Where(f => f.FilterGroup == ValueFilterGroups.Any).OrderBy(f => f.FilterIndex).ToArray();
			var all = filters.Where(f => f.FilterGroup == ValueFilterGroups.All).OrderBy(f => f.FilterIndex).ToArray();
			var none = filters.Where(f => f.FilterGroup == ValueFilterGroups.None).OrderBy(f => f.FilterIndex).ToArray();

			var deleted = string.Empty;

			ListValues(model, filterCount, ValueFilterGroups.Any, any, ref deleted);
			ListValues(model, filterCount, ValueFilterGroups.All, all, ref deleted);
			ListValues(model, filterCount, ValueFilterGroups.None, none, ref deleted);

			if (!string.IsNullOrEmpty(deleted))
			{
				model.Filters.Value = model.Filters.Value.Where(f => f.FilterIdValue != deleted).ToArray();
			}

			// It's possible we've changed a filter from the DefinedKeyValueEditorWindow, so we check for that here.
			if (!string.IsNullOrEmpty(changedFilterId) && model.Filters.Value.Any(f => f.FilterIdValue == changedFilterId))
			{
				changedFilterId = null;
				GUI.changed = true;
			}
		}

		static void ListValues(
			ValueFilterModel model,
			int filterCount,
			ValueFilterGroups group,
			IValueFilterEntryModel[] filters,
			ref string deleted
		)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(group.ToString());

				GUILayout.Label("Append New Filter", GUILayout.ExpandWidth(false));

				Create(
					EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Filter Type -", ValueFilterTypes.Unknown, guiOptions: GUILayout.Width(150f)),
					model,
					group,
					filterCount
				);

				EditorGUIExtensions.PauseChangeCheck();
				{
					if (
						GUILayout.Button(
							new GUIContent("Defines", "Select a predefined key that is updated are listened to by the event system."),
							EditorStyles.miniButton,
							GUILayout.ExpandWidth(false)
						)
					)
					{
						KeyDefinitionEditorWindow.Show(
							null,
							result => CreateDefined(
								model,
								group,
								result
							)
						);	
					}
				}
				EditorGUIExtensions.UnPauseChangeCheck();
			}
			GUILayout.EndHorizontal();

			var isAlternate = false;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(16f);
				GUILayout.BeginVertical();
				{
					foreach (var filter in filters)
					{
						isAlternate = !isAlternate;

						EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, Color.grey.NewV(0.5f), isAlternate);
						{
							switch (filter.FilterType)
							{
								case ValueFilterTypes.KeyValueBoolean:
									OnHandle(filter as BooleanKeyValueFilterEntryModel, ref deleted);
									break;
								case ValueFilterTypes.KeyValueInteger:
									OnHandle(filter as IntegerKeyValueFilterEntryModel, ref deleted);
									break;
								case ValueFilterTypes.KeyValueString:
									OnHandle(filter as StringKeyValueFilterEntryModel, ref deleted);
									break;
								case ValueFilterTypes.KeyValueFloat:
									OnHandle(filter as FloatKeyValueFilterEntryModel, ref deleted);
									break;
								case ValueFilterTypes.KeyValueEnumeration:
									OnHandle(filter as EnumerationKeyValueFilterEntryModel, ref deleted);
									break;
								case ValueFilterTypes.EncounterInteraction:
									OnHandleIdValidated<EncounterInteractionFilterEntryModel, EncounterInteractionFilterOperations, EncounterInfoModel>(
										filter as EncounterInteractionFilterEntryModel,
										new GUIContent("Encounter Id", "The Id of the encounter for this filter."),
										(filter as EncounterInteractionFilterEntryModel).Operation,
										typedFilter => EditorModelMediator.Instance.IsValidModel<EncounterInfoModel>(typedFilter.FilterValue.Value),
										ref deleted
									);
									break;
								case ValueFilterTypes.ModuleTrait:
									OnHandleIdValidated<ModuleTraitFilterEntryModel, ModuleTraitFilterOperations, ModuleTraitModel>(
										filter as ModuleTraitFilterEntryModel,
										new GUIContent("Module Trait Operation Id", "The Id required for the operation specified by this module trait filter."),
										(filter as ModuleTraitFilterEntryModel).Operation,
										typedFilter =>
										{
											switch (typedFilter.Operation.Value)
											{
												case ModuleTraitFilterOperations.PresentFamilyId:
													return EditorModelMediator.Instance.IsValidModuleTraitFamilyId(typedFilter.FilterValue.Value);
												default:
													return EditorModelMediator.Instance.IsValidModel<ModuleTraitModel>(typedFilter.FilterValue.Value);
											}
										},
										ref deleted,
										OnHandleModuleTraitSecondLine
									);
									break;
								default:
									EditorGUILayout.HelpBox("Unrecognized FilterType: " + filter.FilterType, MessageType.Error);
									break;
							}
						}
						GUILayout.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}

		static void CreateDefined(
			ValueFilterModel model,
			ValueFilterGroups group,
			DefinedState replacement
		)
		{
			var valueFilterType = ValueFilterTypes.Unknown;

			switch (replacement.ValueType)
			{
				case KeyValueTypes.Boolean: valueFilterType = ValueFilterTypes.KeyValueBoolean; break;
				case KeyValueTypes.Integer: valueFilterType = ValueFilterTypes.KeyValueInteger; break;
				case KeyValueTypes.String: valueFilterType = ValueFilterTypes.KeyValueString; break;
				case KeyValueTypes.Float: valueFilterType = ValueFilterTypes.KeyValueFloat; break;
				case KeyValueTypes.Enumeration: valueFilterType = ValueFilterTypes.KeyValueEnumeration; break;
				default:
					Debug.LogError("Unrecognized KeyValueType: " + replacement.ValueType);
					break;
			}

			changedFilterId = Create(
				valueFilterType,
				model,
				group,
				model.Filters.Value.Length,
				keyValueCreateInfo: new KeyValueCreateInfo
				{
					ForeignTarget = replacement.Target,
					ForeignKey = replacement.Key
				}
			).FilterIdValue;
		}

		static IValueFilterEntryModel Create(
			ValueFilterTypes type,
			ValueFilterModel model,
			ValueFilterGroups group,
			int index,
			string filterId = null,
			KeyValueCreateInfo keyValueCreateInfo = default(KeyValueCreateInfo)
		)
		{
			switch (type)
			{
				case ValueFilterTypes.Unknown: break;
				case ValueFilterTypes.KeyValueBoolean:
					return OnCreateKeyValue(Create<BooleanKeyValueFilterEntryModel>(model, index, group, filterId), keyValueCreateInfo);
				case ValueFilterTypes.KeyValueInteger:
					return OnCreateKeyValue(Create<IntegerKeyValueFilterEntryModel>(model, index, group, filterId), keyValueCreateInfo);
				case ValueFilterTypes.KeyValueString:
					return OnCreateKeyValue(Create<StringKeyValueFilterEntryModel>(model, index, group, filterId), keyValueCreateInfo);
				case ValueFilterTypes.KeyValueFloat:
					return OnCreateKeyValue(Create<FloatKeyValueFilterEntryModel>(model, index, group, filterId), keyValueCreateInfo);
				case ValueFilterTypes.KeyValueEnumeration:
					return OnCreateKeyValue(Create<EnumerationKeyValueFilterEntryModel>(model, index, group, filterId), keyValueCreateInfo);
				case ValueFilterTypes.EncounterInteraction:
					return Create<EncounterInteractionFilterEntryModel>(model, index, group, filterId);
				case ValueFilterTypes.ModuleTrait:
					return Create<ModuleTraitFilterEntryModel>(model, index, group, filterId);
				default: Debug.LogError("Unrecognized FilterType: " + type); break;
			}
			return null;
		}

		static T Create<T>(
			ValueFilterModel model,
			int index,
			ValueFilterGroups group,
			string filterId = null
		) where T : IValueFilterEntryModel, new()
		{
			var result = new T();
			result.FilterIdValue = string.IsNullOrEmpty(filterId) ? Guid.NewGuid().ToString() : filterId;
			result.FilterIndex = index;
			result.FilterGroup = group;
			model.Filters.Value = model.Filters.Value.Append(result).ToArray();

			return result;
		}

		static KeyValueFilterEntryModel<T> OnCreateKeyValue<T>(
			KeyValueFilterEntryModel<T> entry,
			KeyValueCreateInfo keyValueCreateInfo
		)
			where T: IConvertible
		{
			entry.Input0 = KeyValueAddress<T>.Foreign(keyValueCreateInfo.ForeignTarget, keyValueCreateInfo.ForeignKey);
			entry.Input1 = KeyValueAddress<T>.Local();

			return entry;
		}

		#region Handling
		static void OnOneLineHandleBegin(
			IValueFilterEntryModel model
		)
		{
			GUILayout.BeginHorizontal();

			if (model.FilterIgnore) EditorGUILayoutExtensions.PushColor(Color.gray);

			GUILayout.Box(
				new GUIContent(string.Empty, ObjectNames.NicifyVariableName(model.FilterType.ToString())),
				SubLightEditorConfig.Instance.SharedModelEditorModelsFilterEntryIcon
			);
		}

		static void OnOneLineHandleEnd(IValueFilterEntryModel model, ref string deleted)
		{
			if (model.FilterIgnore) EditorGUILayoutExtensions.PopColor();

			model.FilterIgnore = EditorGUILayout.Toggle(model.FilterIgnore, GUILayout.Width(14f));

			if (!Event.current.shift && Event.current.alt)
			{
				if (EditorGUILayoutExtensions.XButton(true)) deleted = model.FilterIdValue;
			}

			GUILayout.EndHorizontal();
		}

		static void OnHandleKeyValueBegin<T>(
			KeyValueFilterEntryModel<T> model
		)
			where T : IConvertible
		{
			OnOneLineHandleBegin(model);

			EditorGUILayoutKeyDefinition.Value(
				() => model.Input0,
				result => model.Input0 = result,
				KeyValueSources.KeyValue
			);
		}

		static void OnHandleKeyValueEnd(IKeyValueFilterEntryModel model, ref string deleted)
		{
			OnOneLineHandleEnd(model, ref deleted);
		}

		static void OnHandle(
			BooleanKeyValueFilterEntryModel model,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model);
			{
				GUILayout.Label("Equals", GUILayout.Width(OperatorWidth));

				EditorGUILayoutKeyDefinition.Value(
					() => model.Input1,
					result => model.Input1 = result
				);
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(
			IntegerKeyValueFilterEntryModel model,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model);
			{
				model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Select Operation -",
					model.Operation.Value,
					Color.red,
					GUILayout.Width(OperatorWidth)
				);

				EditorGUILayoutKeyDefinition.Value(
					() => model.Input1,
					result => model.Input1 = result
				);
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(
			StringKeyValueFilterEntryModel model,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model);
			{
				model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Select Operation -",
					model.Operation.Value,
					Color.red,
					GUILayout.Width(OperatorWidth)
				);

				switch (model.Operation.Value)
				{
					case StringFilterOperations.Equals:
					case StringFilterOperations.NormalizedEquals:
						EditorGUILayoutKeyDefinition.Value(
							() => model.Input1,
							result => model.Input1 = result
						);
						break;
				}
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(
			FloatKeyValueFilterEntryModel model,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model);
			{
				model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Select Operation -",
					model.Operation.Value,
					Color.red,
					GUILayout.Width(OperatorWidth)
				);

				EditorGUILayoutKeyDefinition.Value(
					() => model.Input1,
					result => model.Input1 = result
				);
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(
			EnumerationKeyValueFilterEntryModel model,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model);
			{
				model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Select Operation -",
					model.Operation.Value,
					Color.red,
					GUILayout.Width(OperatorWidth)
				);

				switch (model.Operation.Value)
				{
					case EnumerationFilterOperations.Equals:
					case EnumerationFilterOperations.NotEquals:
						EditorGUILayoutKeyDefinition.Value(
							() => model.Input1,
							result => model.Input1 = result,
							keyValueOverride: KeyValueTypes.Enumeration,
							keyValueOverrideRelated: model.Input0
						);
						break;
				}
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandleIdValidated<V, O, M>(
			V model,
			GUIContent content,
			ListenerProperty<O> operation,
			Func<V, EditorModelMediator.Validation> getValidation,
			ref string deleted,
			Action<V> handleSecondLine = null
		)
			where V : ValueFilterEntryModel<string>
			where O : struct, Enum
			where M : SaveModel
		{
			OnOneLineHandleBegin(model);
			{
				var validation = getValidation(model);
				
				content.tooltip += "\n" + validation.Tooltip;
				var validationColor = validation.Color;
				
				EditorGUILayoutExtensions.PushColorValidation(validationColor);
				{
					GUILayout.Label(content, GUILayout.ExpandWidth(false));

					model.FilterValue.Value = EditorGUILayout.TextField(model.FilterValue.Value);
				}
				EditorGUILayoutExtensions.PopColorValidation(validationColor);

				if (string.IsNullOrEmpty(model.FilterValue.Value))
				{
					// TODO: Toggle this behaviour if allowed!
					const float CurrentLabelOffset = 53f;
					GUILayout.Space(-CurrentLabelOffset);
					EditorGUILayoutExtensions.PushColor(Color.gray);
					{
						GUILayout.Label("Current", GUILayout.Width(CurrentLabelOffset));
					}
					EditorGUILayoutExtensions.PopColor();
				}

				GUILayout.Label("Needs To Be", GUILayout.Width(OperatorWidth));

				operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Select Operation -",
					operation.Value,
					Color.red
				);
			}
			OnOneLineHandleEnd(model, ref deleted);

			handleSecondLine?.Invoke(model);
		}

		static void OnHandleModuleTraitSecondLine(ModuleTraitFilterEntryModel model)
		{
			GUILayout.Label("Valid Module Types (Select none to check all Module Types)");
			
			EditorGUILayoutExtensions.PushIndent();
			{
				foreach (var moduleType in EnumExtensions.GetValues(ModuleTypes.Unknown))
				{
					var isValidModule = model.ValidModuleTypes.Value.Contains(moduleType);
					var isValidModuleToggle = EditorGUILayout.Toggle(
						ObjectNames.NicifyVariableName(moduleType.ToString()),
						isValidModule
					);

					if (isValidModule != isValidModuleToggle)
					{
						if (isValidModuleToggle) model.ValidModuleTypes.Value = model.ValidModuleTypes.Value.Append(moduleType).ToArray();
						else model.ValidModuleTypes.Value = model.ValidModuleTypes.Value.ExceptOne(moduleType).ToArray();
					}
				}
			}
			EditorGUILayoutExtensions.PopIndent();
		}
		#endregion
	}
}