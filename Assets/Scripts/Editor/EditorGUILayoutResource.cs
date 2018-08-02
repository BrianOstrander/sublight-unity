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
			model.Rations.Value = EditorGUILayout.FloatField("Rations", model.Rations.Value);
			model.Fuel.Value = EditorGUILayout.FloatField("Fuel", model.Fuel.Value);
			model.Speed.Value = EditorGUILayout.FloatField("Speed", model.Speed.Value);
		}
	}
}