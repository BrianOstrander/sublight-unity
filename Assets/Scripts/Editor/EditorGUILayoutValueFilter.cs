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
			if (color.HasValue) EditorGUILayoutExtensions.PushColor(color.Value);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (color.HasValue) EditorGUILayoutExtensions.PopColor();
			{
				GUILayout.Label(content, EditorStyles.boldLabel);
				Values(model);
			}
			GUILayout.EndVertical();
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
				var result = EditorGUILayoutExtensions.HelpfulEnumPopup("- Select Filter Type -", ValueFilterTypes.Unknown, guiOptions: GUILayout.ExpandWidth(false));
				switch (result)
				{
					case ValueFilterTypes.Unknown:
						break;
					case ValueFilterTypes.KeyValueBoolean:
						Create<BooleanKeyValueFilterEntryModel>(model, filterCount, group);
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

						if (isAlternate) EditorGUILayoutExtensions.PushColor(Color.grey.NewV(0.5f));
						GUILayout.BeginVertical(EditorStyles.helpBox);
						if (isAlternate) EditorGUILayoutExtensions.PopColor();
						{
							switch (filter.FilterType)
							{
								case ValueFilterTypes.KeyValueBoolean:
									OnHandle(filter as BooleanKeyValueFilterEntryModel, ref deleted);
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
		static void OnHandle(BooleanKeyValueFilterEntryModel model, ref string deleted)
		{
			GUILayout.BeginHorizontal();
			{
				model.Target.Value = EditorGUILayoutExtensions.HelpfulEnumPopup("- Select Target -", model.Target.Value, guiOptions: GUILayout.ExpandWidth(false));
				model.Key.Value = EditorGUILayout.TextField(model.Key.Value);
				GUILayout.Label("Equals");
				model.FilterValue.Value = EditorGUILayoutExtensions.ToggleButton(model.FilterValue.Value);
				if (EditorGUILayoutExtensions.XButton()) deleted = model.FilterId.Value;
			}
			GUILayout.EndHorizontal();
			if (model.Target.Value == KeyValueTargets.Unknown) EditorGUILayout.HelpBox("A target must be specified.", MessageType.Error);
			if (string.IsNullOrEmpty(model.Key.Value)) EditorGUILayout.HelpBox("A key must be specified.", MessageType.Error);
		}
		#endregion
	}
}