using LunraGames.SubLight.Models;
using LunraGamesEditor;
using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
{
	public static class EditorGUILayoutModel
	{
		public static void Id(SaveModel model) 
		{
			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushEnabled(false);
				{
					EditorGUILayout.TextField("Id", model.Id.Value);
				}
				EditorGUILayoutExtensions.PopEnabled();
			
				EditorGUIExtensions.PauseChangeCheck();
				{
					if (GUILayout.Button("Copy", EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false))) EditorGUIUtility.systemCopyBuffer = model.Id.Value;
				}
				EditorGUIExtensions.UnPauseChangeCheck();
				if (GUILayout.Button("Rename", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false))) Debug.LogWarning("logic for renaming here!");
			}
			GUILayout.EndHorizontal();			
		}
	}
}