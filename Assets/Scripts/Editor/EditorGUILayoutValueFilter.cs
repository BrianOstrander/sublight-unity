﻿using System;
using System.Linq;

using UnityEditor;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGamesEditor;

using DefinedState = LunraGames.SubLight.DefinedKeyValueEditorWindow.DefinedState;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutValueFilter
	{
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
						DefinedKeyValueEditorWindow.Show(
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
								case ValueFilterTypes.EncounterInteraction:
									OnHandle(filter as EncounterInteractionFilterEntryModel, ref deleted);
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
				default:
					Debug.LogError("Unrecognized KeyValueType: " + replacement.ValueType);
					break;
			}

			var result = Create(
				valueFilterType,
				model,
				group,
				model.Filters.Value.Length
			) as IKeyValueFilterEntryModel;

			result.SetOperand(KeyValueSources.KeyValue, replacement.Target, replacement.Key);

			changedFilterId = result.FilterIdValue;
		}

		static IValueFilterEntryModel Create(
			ValueFilterTypes type,
			ValueFilterModel model,
			ValueFilterGroups group,
			int index,
			string filterId = null
		)
		{
			switch (type)
			{
				case ValueFilterTypes.Unknown: break;
				case ValueFilterTypes.KeyValueBoolean:
					return Create<BooleanKeyValueFilterEntryModel>(model, index, group, filterId, OnCreateKeyValue);
				case ValueFilterTypes.KeyValueInteger:
					return Create<IntegerKeyValueFilterEntryModel>(model, index, group, filterId, OnCreateKeyValue);
				case ValueFilterTypes.KeyValueString:
					return Create<StringKeyValueFilterEntryModel>(model, index, group, filterId, OnCreateKeyValue);
				case ValueFilterTypes.KeyValueFloat:
					return Create<FloatKeyValueFilterEntryModel>(model, index, group, filterId, OnCreateKeyValue);
				case ValueFilterTypes.EncounterInteraction:
					return Create<EncounterInteractionFilterEntryModel>(model, index, group, filterId);
				default: Debug.LogError("Unrecognized FilterType: " + type); break;
			}
			return null;
		}

		static IValueFilterEntryModel Create<T>(
			ValueFilterModel model,
			int index,
			ValueFilterGroups group,
			string filterId = null,
			Func<IValueFilterEntryModel, IValueFilterEntryModel> initialize = null
		) where T : IValueFilterEntryModel, new()
		{
			var result = new T();
			result.FilterIdValue = string.IsNullOrEmpty(filterId) ? Guid.NewGuid().ToString() : filterId;
			result.FilterIndex = index;
			result.FilterGroup = group;
			model.Filters.Value = model.Filters.Value.Append(result).ToArray();

			if (initialize == null) return result;
			return initialize(result);
		}

		static IValueFilterEntryModel OnCreateKeyValue(IValueFilterEntryModel result)
		{
			var typedResult = result as IKeyValueFilterEntryModel;

			typedResult.SetInput(KeyValueSources.LocalValue, KeyValueTargets.Unknown, null);

			return typedResult;
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

		static void OnHandleKeyValueBegin(
			IKeyValueFilterEntryModel model
		)
		{
			OnOneLineHandleBegin(model);

			EditorGUILayoutDefinedKeyValue.OnHandleKeyValueDefinition(
				ObjectNames.NicifyVariableName(model.FilterType.ToString()),
				model.FilterIdValue,
				model.FilterValueType,
				model.OperandAddress.ForeignTarget,
				model.OperandAddress.ForeignKey,
				target => model.SetOperand(KeyValueSources.KeyValue, target, model.OperandAddress.ForeignKey),
				key => model.SetOperand(KeyValueSources.KeyValue, model.OperandAddress.ForeignTarget, key),
				() => changedFilterId = model.FilterIdValue,
				KeyValueKeyWidth
			);
		}

		static void OnHandleKeyValueEnd(IKeyValueFilterEntryModel model, ref string deleted)
		{
			OnOneLineHandleEnd(model, ref deleted);
		}

		/// <summary>
		/// Handles the selection and validation of defined key value sources.
		/// </summary>
		/// <returns><c>true</c>, if this method handled the value, <c>false</c> otherwise.</returns>
		/// <param name="model">Model.</param>
		static bool OnHandleKeyValueSource(IKeyValueFilterEntryModel model)
		{
			var inputSource = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				GUIContent.none,
				"- Source -",
				model.InputAddress.Source,
				Color.red,
				guiOptions: GUILayout.Width(70f)
			);

			model.SetInput(inputSource, model.InputAddress.ForeignTarget, model.InputAddress.ForeignKey);

			switch (model.InputAddress.Source)
			{
				case KeyValueSources.LocalValue:
					return false;
				case KeyValueSources.KeyValue:
					EditorGUILayoutDefinedKeyValue.OnHandleKeyValueDefinition(
						ObjectNames.NicifyVariableName(model.FilterType.ToString()),
						model.FilterIdValue,
						model.FilterValueType,
						model.InputAddress.ForeignTarget,
						model.InputAddress.ForeignKey,
						target => model.SetInput(KeyValueSources.KeyValue, target, model.InputAddress.ForeignKey),
						key => model.SetInput(KeyValueSources.KeyValue, model.InputAddress.ForeignTarget, key),
						() => changedFilterId = model.FilterIdValue
					);
					break;
				default:
					EditorGUILayoutExtensions.PushColor(Color.red.NewS(0.65f));
					{
						GUILayout.Label("Unrecognized Source: " + model.InputAddress.Source, GUILayout.ExpandWidth(false));
					}
					EditorGUILayoutExtensions.PopColor();
					break;
			}

			return true;
		}

		static void OnHandle(
			BooleanKeyValueFilterEntryModel model,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model);
			{
				GUILayout.Label("Equals", GUILayout.Width(OperatorWidth));

				if(!OnHandleKeyValueSource(model))
				{
					var result = model.Input.Value;
					result.LocalValue = EditorGUILayoutExtensions.ToggleButtonValue(result.LocalValue, style: EditorStyles.miniButton);
					model.Input.Value = result;
				}
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

				if (!OnHandleKeyValueSource(model))
				{
					var result = model.Input.Value;
					result.LocalValue = EditorGUILayout.IntField(result.LocalValue);
					model.Input.Value = result;
				}
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

				if (model.Operation.Value == StringFilterOperations.Equals)
				{
					if (!OnHandleKeyValueSource(model))
					{
						var result = model.Input.Value;
						result.LocalValue = EditorGUILayout.TextField(result.LocalValue);
						model.Input.Value = result;
					}
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

				if (!OnHandleKeyValueSource(model))
				{
					var result = model.Input.Value;
					result.LocalValue = EditorGUILayout.FloatField(result.LocalValue);
					model.Input.Value = result;
				}
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(
			EncounterInteractionFilterEntryModel model,
			ref string deleted
		)
		{
			OnOneLineHandleBegin(model);
			{
				GUILayout.Label(new GUIContent("Encounter Id", "The Id of the encounter for this filter."), GUILayout.ExpandWidth(false));

				model.FilterValue.Value = EditorGUILayout.TextField(model.FilterValue.Value);

				if (string.IsNullOrEmpty(model.FilterValue.Value))
				{
					const float CurrentLabelOffset = 53f;
					GUILayout.Space(-CurrentLabelOffset);
					EditorGUILayoutExtensions.PushColor(Color.gray);
					{
						GUILayout.Label("Current", GUILayout.Width(CurrentLabelOffset));
					}
					EditorGUILayoutExtensions.PopColor();
				}

				GUILayout.Label("Needs To Be", GUILayout.Width(OperatorWidth));

				model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValueValidation(
					"- Select Operation -",
					model.Operation.Value,
					Color.red
				);
			}
			OnOneLineHandleEnd(model, ref deleted);
		}
		#endregion
	}
}