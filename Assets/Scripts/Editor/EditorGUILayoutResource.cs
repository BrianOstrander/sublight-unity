using UnityEditor;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGamesEditor;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutResource
	{
		public static void Field(string name, ResourceInventoryModel model, Color? color = null)
		{
			Field(new GUIContent(name), model, color);
		}

		public static void Field(GUIContent content, ResourceInventoryModel model, Color? color = null)
		{
			EditorGUILayoutExtensions.BeginVertical(EditorStyles.helpBox, color, color.HasValue);
			{
				GUILayout.Label(content, EditorStyles.boldLabel);
				Values(model);
			}
			EditorGUILayoutExtensions.EndVertical();
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