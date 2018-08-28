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
					EditorGUIExtensions.PauseChangeCheck();
					{
						model.ShowValues.Value = EditorGUILayout.Foldout(model.ShowValues.Value, content);
					}
					EditorGUIExtensions.UnPauseChangeCheck();
					if (noFilters) EditorGUILayoutExtensions.PopColor();

					GUILayout.Label("Default", GUILayout.ExpandWidth(false));
					model.FalseByDefault.Value = !EditorGUILayoutExtensions.ToggleButtonValue(!model.FalseByDefault.Value);
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
					case ValueFilterTypes.KeyValueString:
						Create<StringKeyValueFilterEntryModel>(model, filterCount, group);
						break;
					case ValueFilterTypes.EncounterInteraction:
						Create<EncounterInteractionFilterEntryModel>(model, filterCount, group);
						break;
					case ValueFilterTypes.InventoryId:
						Create<InventoryIdFilterEntryModel>(model, filterCount, group);
						break;
					case ValueFilterTypes.InventoryTag:
						Create<InventoryTagFilterEntryModel>(model, filterCount, group);
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
								case ValueFilterTypes.KeyValueString:
									OnHandle(filter as StringKeyValueFilterEntryModel, ref deleted);
									break;
								case ValueFilterTypes.EncounterInteraction:
									OnHandle(filter as EncounterInteractionFilterEntryModel, ref deleted);
									break;
								case ValueFilterTypes.InventoryId:
									OnHandle(filter as InventoryIdFilterEntryModel, ref deleted);
									break;
								case ValueFilterTypes.InventoryTag:
									OnHandle(filter as InventoryTagFilterEntryModel, ref deleted);
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

			model.FilterIgnore = EditorGUILayout.Toggle(model.FilterIgnore, GUILayout.Width(14f));
			if (model.FilterIgnore) EditorGUILayoutExtensions.PushColor(Color.gray);

		}

		static void OnOneLineHandleEnd(IValueFilterEntryModel model, ref string deleted)
		{
			if (EditorGUILayoutExtensions.XButton()) deleted = model.FilterIdValue;
			if (model.FilterIgnore) EditorGUILayoutExtensions.PopColor();

			GUILayout.EndHorizontal();
		}

		static void OnHandleKeyValueBegin(IKeyValueFilterEntryModel model)
		{
			OnOneLineHandleBegin(model);

			model.FilterKeyTarget = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Target -", model.FilterKeyTarget, guiOptions: GUILayout.ExpandWidth(false));
			model.FilterKey = EditorGUILayout.TextField(model.FilterKey);
		}

		static void OnHandleKeyValueEnd(IKeyValueFilterEntryModel model, ref string deleted)
		{
			OnOneLineHandleEnd(model, ref deleted);

			if (model.FilterKeyTarget == KeyValueTargets.Unknown) EditorGUILayout.HelpBox("A target must be specified.", MessageType.Error);
			if (string.IsNullOrEmpty(model.FilterKey)) EditorGUILayout.HelpBox("A key must be specified.", MessageType.Error);
		}

		static void OnHandle(BooleanKeyValueFilterEntryModel model, ref string deleted)
		{
			OnHandleKeyValueBegin(model);
			{
				GUILayout.Label("Equals");
				model.FilterValue.Value = EditorGUILayoutExtensions.ToggleButtonValue(model.FilterValue.Value);
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(StringKeyValueFilterEntryModel model, ref string deleted)
		{
			OnHandleKeyValueBegin(model);
			{
				model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Operation -", model.Operation.Value, guiOptions: GUILayout.ExpandWidth(false));
				if (model.Operation.Value == StringFilterOperations.Equals)
				{
					model.FilterValue.Value = EditorGUILayout.TextField(model.FilterValue.Value);
				}
			}
			OnHandleKeyValueEnd(model, ref deleted);
		}

		static void OnHandle(EncounterInteractionFilterEntryModel model, ref string deleted)
		{
			OnOneLineHandleBegin(model);
			{
				GUILayout.Label(new GUIContent("EncounterId", "The Id of the encounter for this filter."), GUILayout.ExpandWidth(false));
				model.FilterValue.Value = EditorGUILayout.TextField(model.FilterValue.Value);
				GUILayout.Label("Needs To Be");
				model.Operation.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValue("- Select Operation -", model.Operation.Value);
			}
			OnOneLineHandleEnd(model, ref deleted);

			if (string.IsNullOrEmpty(model.FilterValue.Value)) EditorGUILayout.HelpBox("An EncounterId must be specified.", MessageType.Error);
			if (model.Operation.Value == EncounterInteractionFilterOperations.Unknown) EditorGUILayout.HelpBox("An operation must be specified.", MessageType.Error);
		}

		static void OnHandle(InventoryIdFilterEntryModel model, ref string deleted)
		{
			OnOneLineHandleBegin(model);
			{
				GUILayout.Label(new GUIContent("InventoryId", "The Id of the inventory reference."), GUILayout.ExpandWidth(false));
				model.FilterValue.Value = EditorGUILayout.TextField(model.FilterValue.Value);
			}
			OnOneLineHandleEnd(model, ref deleted);

			if (string.IsNullOrEmpty(model.FilterValue.Value)) EditorGUILayout.HelpBox("An InventoryId must be specified.", MessageType.Error);
		}

		static void OnHandle(InventoryTagFilterEntryModel model, ref string deleted)
		{
			OnOneLineHandleBegin(model);
			{
				GUILayout.Label(new GUIContent("Tag", "The name of the tag for this filter."), GUILayout.ExpandWidth(false));
				model.FilterValue.Value = EditorGUILayout.TextField(model.FilterValue.Value);
			}
			OnOneLineHandleEnd(model, ref deleted);

			if (string.IsNullOrEmpty(model.FilterValue.Value)) EditorGUILayout.HelpBox("A tag must be specified.", MessageType.Error);
		}
		#endregion
	}
}