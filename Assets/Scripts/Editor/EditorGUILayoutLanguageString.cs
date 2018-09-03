using System.Linq;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutLanguageString
	{
		static LanguageDatabaseModel Current;
		static bool wasWarned;

		public static void BeginLanguage(LanguageDatabaseModel model) { Current = model; }

		public static void EndLanguage() { Current = null; }

		public static void Field(string name, LanguageStringModel model, string temp)
		{
			Field(new GUIContent(name), model, temp);
		}

		public static void Field(GUIContent content, LanguageStringModel model, string temp)
		{
			content.tooltip = string.IsNullOrEmpty(content.tooltip) ? model.Key : content.tooltip;

			BeginCheck(model);
			{
				var edge = Current == null ? null : Current.Language.Edges.Value.FirstOrDefault(e => e.Key.Value == model.Key.Value);
				var duplicates = edge == null ? new LanguageDatabaseEdge[0] : Current.Language.GetDuplicates(edge);

				var hasColor = edge != null && edge.IsDuplicate;

				if (hasColor) EditorGUILayoutExtensions.PushColor(edge.IsDuplicateSource ? Color.yellow.NewB(0.75f) : Color.yellow);
				{
					var original = model.Value.Value;
					var result = EditorGUILayoutExtensions.TextDynamic(
						content,
						original
					);
					model.ShowDetails.Value = EditorGUILayout.Foldout(model.ShowDetails.Value, "Details");
					if (model.ShowDetails.Value)
					{
						GUILayout.BeginHorizontal();
						{
							GUILayout.Space(18f);
							GUILayout.BeginVertical();
							{
								Details(model, edge, duplicates);
							}
							GUILayout.EndVertical();
						}
						GUILayout.EndHorizontal();
					}

					if (!string.IsNullOrEmpty(temp))
					{
						model.Value.Value = temp;
						model.HasUnsavedValue = Current == null;
						if (Current != null)
						{
							if (edge == null)
							{
								edge = new LanguageDatabaseEdge();
								edge.Key.Value = model.Key.Value;
								edge.Value.Value = result;
								Current.Language.Edges.Value = Current.Language.Edges.Value.Append(edge).ToArray();
							}
							else
							{
								edge.Value.Value = result;
								Current.Language.Edges.Value = Current.Language.Edges.Value.ToArray();
							}
						}
					}
					else if (original != result || model.HasUnsavedValue)
					{
						model.Value.Value = result;
						model.HasUnsavedValue = Current == null;
						if (Current != null)
						{
							if (edge == null)
							{
								edge = new LanguageDatabaseEdge();
								edge.Key.Value = model.Key.Value;
								edge.Value.Value = result;
								Current.Language.Edges.Value = Current.Language.Edges.Value.Append(edge).ToArray();
							}
							else
							{
								edge.Value.Value = result;
								Current.Language.Edges.Value = Current.Language.Edges.Value.ToArray();
							}
						}
					}
				}
				if (hasColor) EditorGUILayoutExtensions.PopColor();
			}
			EndCheck();
		}

		static void Details(LanguageStringModel model, LanguageDatabaseEdge edge, LanguageDatabaseEdge[] duplicates)
		{
			var labels = new List<string>();
			var options = new List<string>();
			labels.Add("- Select Key -");
			options.Add(null);

			foreach (var duplicate in duplicates)
			{
				var label = duplicate.Key.Value;
				if (!string.IsNullOrEmpty(duplicate.Notes.Value))
				{
					label = duplicate.Notes.Value + " | " + label.Substring(0, Mathf.Min(label.Length, 8));
				}
				labels.Add(label);
				options.Add(duplicate.Key.Value);
			}

			var selected = EditorGUILayout.Popup("Merge Duplicate", 0, labels.ToArray());
			if (0 < selected) model.Key.Value = options[selected];

			if (edge == null)
			{
				EditorGUILayoutExtensions.TextDynamic("Notes", null, leftOffset: false);
			}
			else edge.Notes.Value = EditorGUILayoutExtensions.TextDynamic("Notes", edge.Notes.Value, leftOffset: false);
		}

		static void BeginCheck(LanguageStringModel model)
		{
			wasWarned = Current == null || model.HasUnsavedValue;
			if (wasWarned) EditorGUILayoutExtensions.PushColor(model.HasUnsavedValue ? Color.yellow : Color.red);
		}

		static void EndCheck()
		{
			if (wasWarned) EditorGUILayoutExtensions.PopColor();
		}
	}
}