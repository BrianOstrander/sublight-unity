using System;
using System.Linq;

using UnityEditor;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGamesEditor;

using ReplaceResult = LunraGames.SubLight.DefinedKeyValueEditorWindow.ReplaceResult;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutValueFilter
	{
		struct DefinedKeyEntry
		{
			public bool Current;
			public KeyValueTargets Target;
			public string Key;
			public string Notes;
		}

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
							result => ReplaceValue(
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

			Action<ReplaceResult> onReplacement = replacement => ReplaceValue(model, group, replacement);
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
									OnHandle(filter as BooleanKeyValueFilterEntryModel, onReplacement, ref deleted);
									break;
								case ValueFilterTypes.KeyValueInteger:
									OnHandle(filter as IntegerKeyValueFilterEntryModel, onReplacement, ref deleted);
									break;
								case ValueFilterTypes.KeyValueString:
									OnHandle(filter as StringKeyValueFilterEntryModel, onReplacement, ref deleted);
									break;
								case ValueFilterTypes.KeyValueFloat:
									OnHandle(filter as FloatKeyValueFilterEntryModel, onReplacement, ref deleted);
									break;
								case ValueFilterTypes.EncounterInteraction:
									OnHandle(filter as EncounterInteractionFilterEntryModel, onReplacement, ref deleted);
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

		static void ReplaceValue(
			ValueFilterModel model,
			ValueFilterGroups group,
			ReplaceResult replacement
		)
		{
			if (replacement.Previous == null)
			{
				changedFilterId = Create(
					replacement.FilterType,
					model,
					group,
					model.Filters.Value.Length
				).FilterIdValue;
				return;
			}

			model.Filters.Value = model.Filters.Value.Where(f => f.FilterIdValue != replacement.Previous.FilterIdValue).ToArray();

			changedFilterId = Create(
				replacement.FilterType,
				model,
				group,
				replacement.Previous.FilterIndex,
				replacement.Previous.FilterIdValue
			).FilterIdValue;

			var instance = model.Filters.Value.First(f => f.FilterIdValue == replacement.Previous.FilterIdValue);

			if (instance is IKeyValueFilterEntryModel)
			{
				var keyValueInstance = instance as IKeyValueFilterEntryModel;
				keyValueInstance.FilterKeyTarget = replacement.Target;
				keyValueInstance.FilterKey = replacement.Key;
			}
			else Debug.LogError("Unrecognized filter type: " + instance.GetType().FullName);
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
					return Create<BooleanKeyValueFilterEntryModel>(model, index, group, filterId);
				case ValueFilterTypes.KeyValueInteger:
					return Create<IntegerKeyValueFilterEntryModel>(model, index, group, filterId);
				case ValueFilterTypes.KeyValueString:
					return Create<StringKeyValueFilterEntryModel>(model, index, group, filterId);
				case ValueFilterTypes.KeyValueFloat:
					return Create<FloatKeyValueFilterEntryModel>(model, index, group, filterId);
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

		#region Handling
		static void OnOneLineHandleBegin(
			IValueFilterEntryModel model,
			Action<ReplaceResult> replaced
		)
		{
			GUILayout.BeginHorizontal();

			if (model.FilterIgnore) EditorGUILayoutExtensions.PushColor(Color.gray);

			Action onClick = null;
			var tooltip = ObjectNames.NicifyVariableName(model.FilterType.ToString());
			var style = SubLightEditorConfig.Instance.SharedModelEditorModelsFilterEntryIcon;

			if (DefinedKeyInstances.SupportedFilterTypes.Contains(model.FilterType))
			{
				var notes = string.Empty;

				var keyValueModel = model as IKeyValueFilterEntryModel;

				OnOneLineHandleBeginLinked(
					keyValueModel,
					out style,
					out notes
				);

				tooltip = tooltip + " - Click to link to a defined key value." + (string.IsNullOrEmpty(notes) ? string.Empty : "\n" + notes);

				onClick = () => DefinedKeyValueEditorWindow.Show(
					keyValueModel,
					result => replaced(result)
				);
			}

			EditorGUIExtensions.PauseChangeCheck();
			{
				if (GUILayout.Button(new GUIContent(string.Empty, tooltip), style, GUILayout.ExpandWidth(false)) && onClick != null)
				{
					onClick();
				}
			}
			EditorGUIExtensions.UnPauseChangeCheck();
		}

		static void OnOneLineHandleBeginLinked(
			IKeyValueFilterEntryModel model,
			out GUIStyle style,
			out string help
		)
		{
			style = SubLightEditorConfig.Instance.SharedModelEditorModelsFilterEntryIconNotLinked;
			help = string.Empty;

			if (model.FilterKeyTarget == KeyValueTargets.Unknown) return;
			if (string.IsNullOrEmpty(model.FilterKey)) return;

			DefinedKeys definitions = null;

			switch (model.FilterKeyTarget)
			{
				case KeyValueTargets.Encounter: definitions = DefinedKeyInstances.Encounter; break;
				case KeyValueTargets.Game: definitions = DefinedKeyInstances.Game; break;
				case KeyValueTargets.Global: definitions = DefinedKeyInstances.Global; break;
				case KeyValueTargets.Preferences: definitions = DefinedKeyInstances.Preferences; break;
				default:
					Debug.LogError("Unrecognized KeyValueTarget: " + model.FilterKeyTarget);
					return;
			}

			IDefinedKey[] keys = null;

			switch (model.FilterType)
			{
				case ValueFilterTypes.KeyValueBoolean: keys = definitions.Booleans; break;
				case ValueFilterTypes.KeyValueInteger: keys = definitions.Integers; break;
				case ValueFilterTypes.KeyValueString: keys = definitions.Strings; break;
				case ValueFilterTypes.KeyValueFloat: keys = definitions.Floats; break;
				default:
					Debug.LogError("Unrecognized ValueFilterType: " + model.FilterType);
					return;
			}

			if (keys == null)
			{
				Debug.LogError("Keys should not be null");
				return;
			}

			var linkedKey = keys.FirstOrDefault(k => k.Key == model.FilterKey);

			if (linkedKey == null) return;

			style = SubLightEditorConfig.Instance.SharedModelEditorModelsFilterEntryIconLinked;
			help = linkedKey.Notes ?? string.Empty;
		}

		static void OnOneLineHandleEnd(IValueFilterEntryModel model, ref string deleted)
		{
			if (model.FilterIgnore) EditorGUILayoutExtensions.PopColor();

			model.FilterIgnore = EditorGUILayout.Toggle(model.FilterIgnore, GUILayout.Width(14f));

			if (Event.current.alt)
			{
				if (EditorGUILayoutExtensions.XButton(true)) deleted = model.FilterIdValue;
			}

			GUILayout.EndHorizontal();
		}

		static void OnHandleKeyValueBegin(
			IKeyValueFilterEntryModel model,
			Action<ReplaceResult> replaced,
			string prefix = null
		)
		{
			OnOneLineHandleBegin(model, replaced);

			Color? targetColor = null;

			if (model.FilterKeyTarget == KeyValueTargets.Unknown) targetColor = Color.red;

			if (targetColor.HasValue) EditorGUILayoutExtensions.PushColorCombined(targetColor.Value.NewS(0.25f), targetColor.Value.NewS(0.65f));
			{
				model.FilterKeyTarget = EditorGUILayoutExtensions.HelpfulEnumPopupValue(
					"- Select Target -",
					model.FilterKeyTarget,
					guiOptions: GUILayout.Width(100f)
				);
			}
			if (targetColor.HasValue) EditorGUILayoutExtensions.PopColorCombined();

			Color? keyColor = null;

			if (string.IsNullOrEmpty(model.FilterKey)) keyColor = Color.red;

			if (keyColor.HasValue) EditorGUILayoutExtensions.PushColorCombined(keyColor.Value.NewS(0.25f), keyColor.Value.NewS(0.65f));
			{
				model.FilterKey = EditorGUILayout.TextField(model.FilterKey);
			}
			if (keyColor.HasValue) EditorGUILayoutExtensions.PopColorCombined();
		}

		static void OnHandleKeyValueEnd(IKeyValueFilterEntryModel model, ref string deleted)
		{
			OnOneLineHandleEnd(model, ref deleted);
		}

		static void OnHandle(
			BooleanKeyValueFilterEntryModel model,
			Action<ReplaceResult> replaced,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model, replaced);
			{
				GUILayout.Label("Equals", GUILayout.ExpandWidth(false));
				model.FilterValue.Value = EditorGUILayoutExtensions.ToggleButtonValue(model.FilterValue.Value, style: EditorStyles.miniButton);
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(
			IntegerKeyValueFilterEntryModel model,
			Action<ReplaceResult> replaced,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model, replaced);
			{
				Color? operatorColor = null;

				if (model.Operation.Value == IntegerFilterOperations.Unknown) operatorColor = Color.red;

				if (operatorColor.HasValue) EditorGUILayoutExtensions.PushColorCombined(operatorColor.Value.NewS(0.25f), operatorColor.Value.NewS(0.65f));
				{
					model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Operation -", model.Operation.Value, guiOptions: GUILayout.ExpandWidth(false));
				}
				if (operatorColor.HasValue) EditorGUILayoutExtensions.PopColorCombined();

				model.FilterValue.Value = EditorGUILayout.IntField(model.FilterValue.Value);
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(
			StringKeyValueFilterEntryModel model,
			Action<ReplaceResult> replaced,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model, replaced);
			{
				Color? operatorColor = null;

				if (model.Operation.Value == StringFilterOperations.Unknown) operatorColor = Color.red;

				if (operatorColor.HasValue) EditorGUILayoutExtensions.PushColorCombined(operatorColor.Value.NewS(0.25f), operatorColor.Value.NewS(0.65f));
				{
					model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Operation -", model.Operation.Value, guiOptions: GUILayout.ExpandWidth(false));
				}
				if (operatorColor.HasValue) EditorGUILayoutExtensions.PopColorCombined();

				if (model.Operation.Value == StringFilterOperations.Equals)
				{
					model.FilterValue.Value = EditorGUILayout.TextField(model.FilterValue.Value);
				}
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(
			FloatKeyValueFilterEntryModel model,
			Action<ReplaceResult> replaced,
			ref string deleted
		)
		{
			OnHandleKeyValueBegin(model, replaced);
			{
				Color? operatorColor = null;

				if (model.Operation.Value == FloatFilterOperations.Unknown) operatorColor = Color.red;

				if (operatorColor.HasValue) EditorGUILayoutExtensions.PushColorCombined(operatorColor.Value.NewS(0.25f), operatorColor.Value.NewS(0.65f));
				{
					model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Operation -", model.Operation.Value, guiOptions: GUILayout.ExpandWidth(false));
				}
				if (operatorColor.HasValue) EditorGUILayoutExtensions.PopColorCombined();

				model.FilterValue.Value = EditorGUILayout.FloatField(model.FilterValue.Value);
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(
			EncounterInteractionFilterEntryModel model,
			Action<ReplaceResult> replaced,
			ref string deleted
		)
		{
			OnOneLineHandleBegin(model, replaced);
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

				GUILayout.Label("Needs To Be", GUILayout.ExpandWidth(false));

				Color? operatorColor = null;

				if (model.Operation.Value == EncounterInteractionFilterOperations.Unknown) operatorColor = Color.red;

				if (operatorColor.HasValue) EditorGUILayoutExtensions.PushColorCombined(operatorColor.Value.NewS(0.25f), operatorColor.Value.NewS(0.65f));
				{
					model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Operation -", model.Operation.Value);
				}
				EditorGUILayoutExtensions.PopColorCombined();
			}
			OnOneLineHandleEnd(model, ref deleted);
		}
		#endregion
	}
}