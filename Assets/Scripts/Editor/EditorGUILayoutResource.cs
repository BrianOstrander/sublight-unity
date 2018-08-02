using UnityEditor;

using UnityEngine;

using LunraGames.SpaceFarm.Models;
using LunraGamesEditor;

namespace LunraGames.SpaceFarm
{
	public static class EditorGUILayoutResource
	{
		public static void Field(string name, ResourceInventoryModel model, Color? color = null)
		{
			Field(new GUIContent(name), model, color);
		}

		public static void Field(GUIContent content, ResourceInventoryModel model, Color? color = null)
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

		public static void Values(ResourceInventoryModel model)
		{
			foreach (var value in model.Values)
			{
				value.Value = EditorGUILayout.FloatField(value.Name, value.Value);
			}
		}
	}
}