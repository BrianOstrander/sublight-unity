using System;
using System.Linq;

using UnityEditor;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGamesEditor;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutValueFilter
	{
		static readonly ValueFilterTypes[] LinkableValueFilterTypes = {
			ValueFilterTypes.KeyValueBoolean,
			ValueFilterTypes.KeyValueInteger,
			ValueFilterTypes.KeyValueString,
			ValueFilterTypes.KeyValueFloat
		};

		static EncounterKeys Encounter = new EncounterKeys();
		static GameKeys Game = new GameKeys();
		static GlobalKeys Global = new GlobalKeys();
		static PreferencesKeys Preferences = new PreferencesKeys();

		struct DefinedKeyEntry
		{
			public bool Current;
			public KeyValueTargets Target;
			public string Key;
			public string Notes;
		}

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
						//GUILayout.FlexibleSpace();
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
		}

		static void ListValues(ValueFilterModel model, int filterCount, ValueFilterGroups group, IValueFilterEntryModel[] filters, ref string deleted)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(group.ToString());

				GUILayout.Label("Append New Filter", GUILayout.ExpandWidth(false));
				var result = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Filter Type -", ValueFilterTypes.Unknown, guiOptions: GUILayout.ExpandWidth(false));
				switch (result)
				{
					case ValueFilterTypes.Unknown:
						break;
					case ValueFilterTypes.KeyValueBoolean:
						Create<BooleanKeyValueFilterEntryModel>(model, filterCount, group);
						break;
					case ValueFilterTypes.KeyValueInteger:
						Create<IntegerKeyValueFilterEntryModel>(model, filterCount, group);
						break;
					case ValueFilterTypes.KeyValueString:
						Create<StringKeyValueFilterEntryModel>(model, filterCount, group);
						break;
					case ValueFilterTypes.KeyValueFloat:
						Create<FloatKeyValueFilterEntryModel>(model, filterCount, group);
						break;
					case ValueFilterTypes.EncounterInteraction:
						Create<EncounterInteractionFilterEntryModel>(model, filterCount, group);
						break;
					default:
						Debug.LogError("Unrecognized FilterType: " + result);
						break;
				}
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

		static void Create<T>(ValueFilterModel model, int index, ValueFilterGroups group) where T : IValueFilterEntryModel, new()
		{
			var result = new T();
			result.FilterIdValue = Guid.NewGuid().ToString();
			result.FilterIndex = index;
			result.FilterGroup = group;
			model.Filters.Value = model.Filters.Value.Append(result).ToArray();
		}

		#region Handling
		static void OnOneLineHandleBegin(IValueFilterEntryModel model)
		{
			GUILayout.BeginHorizontal();

			if (model.FilterIgnore) EditorGUILayoutExtensions.PushColor(Color.gray);

			Action onClick = null;
			var tooltip = ObjectNames.NicifyVariableName(model.FilterType.ToString());
			var style = SubLightEditorConfig.Instance.SharedModelEditorModelsFilterEntryIcon;

			if (LinkableValueFilterTypes.Contains(model.FilterType))
			{
				var notes = string.Empty;
				var options = new DefinedKeyEntry[0];

				OnOneLineHandleBeginLinked(
					model as IKeyValueFilterEntryModel,
					out style,
					out notes,
					out options
				);

				tooltip = tooltip + " - Click to link to a defined key value." + (string.IsNullOrEmpty(notes) ? string.Empty : "\n" + notes);

				if (options.Any())
				{
					onClick = () => Debug.Log(options.Length+" TODO OPEN A WINDOW!!!");
				}
			}

			if (onClick == null) EditorGUIExtensions.PauseChangeCheck();
			{
				if (GUILayout.Button(
					new GUIContent(string.Empty, tooltip),
					style,
					GUILayout.ExpandWidth(false)
				) && onClick != null) onClick();
			}
			if (onClick == null) EditorGUIExtensions.UnPauseChangeCheck();
		}

		static void OnOneLineHandleBeginLinked(
			IKeyValueFilterEntryModel model,
			out GUIStyle style,
			out string help,
			out DefinedKeyEntry[] options
		)
		{
			style = SubLightEditorConfig.Instance.SharedModelEditorModelsFilterEntryIconNotLinked;
			help = string.Empty;
			options = new DefinedKeyEntry[0];

			if (model.FilterKeyTarget == KeyValueTargets.Unknown) return;
			if (string.IsNullOrEmpty(model.FilterKey)) return;

			DefinedKeys definitions = null;

			switch (model.FilterKeyTarget)
			{
				case KeyValueTargets.Encounter: definitions = Encounter; break;
				case KeyValueTargets.Game: definitions = Game; break;
				case KeyValueTargets.Global: definitions = Global; break;
				case KeyValueTargets.Preferences: definitions = Preferences; break;
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

			options = keys.Select(
				k => new DefinedKeyEntry
				{
					Current = linkedKey != null && linkedKey.Key == k.Key,
					Key = k.Key,
					Notes = k.Notes
				}
			).ToArray();

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

		static void OnHandleKeyValueBegin(IKeyValueFilterEntryModel model, string prefix = null)
		{
			OnOneLineHandleBegin(model);

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

		static void OnHandle(BooleanKeyValueFilterEntryModel model, ref string deleted)
		{
			OnHandleKeyValueBegin(model);
			{
				GUILayout.Label("Equals", GUILayout.ExpandWidth(false));
				model.FilterValue.Value = EditorGUILayoutExtensions.ToggleButtonValue(model.FilterValue.Value, style: EditorStyles.miniButton);
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(IntegerKeyValueFilterEntryModel model, ref string deleted)
		{
			OnHandleKeyValueBegin(model);
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

		static void OnHandle(StringKeyValueFilterEntryModel model, ref string deleted)
		{
			OnHandleKeyValueBegin(model);
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

		static void OnHandle(FloatKeyValueFilterEntryModel model, ref string deleted)
		{
			OnHandleKeyValueBegin(model);
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

		static void OnHandle(EncounterInteractionFilterEntryModel model, ref string deleted)
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