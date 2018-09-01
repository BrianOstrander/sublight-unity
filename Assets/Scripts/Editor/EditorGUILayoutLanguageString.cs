using System;

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
			BeginCheck(model);
			{
				var original = model.Value.Value;
				var result = EditorGUILayoutExtensions.TextDynamic(content, original);
				if (!string.IsNullOrEmpty(temp))
				{
					model.Value.Value = temp;
					model.HasUnsavedValue = Current == null;
					if (Current != null) Current.Language.Set(model.Key.Value, result);
				}
				else if (original != result || model.HasUnsavedValue)
				{
					model.Value.Value = result;
					model.HasUnsavedValue = Current == null;
					if (Current != null) Current.Language.Set(model.Key.Value, result);
				}
			}
			EndCheck();
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