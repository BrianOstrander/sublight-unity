using UnityEditor;
using UnityEngine;
using LunraGames.SpaceFarm;

namespace LunraGames.SpaceFarm
{
	public static class XButtonSharedEditor
	{
		public static void DrawAutoStyleWarnings()
		{
			if (DevPrefs.ApplyXButtonStyleInEditMode) return;
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.HelpBox("Styles are not currently being applied in edit mode", MessageType.Warning);
				if (GUILayout.Button("Enable Auto Apply", GUILayout.Height(38f)))
				{
					DevPrefs.ApplyXButtonStyleInEditMode.Value = true;
				}
			}
			GUILayout.EndHorizontal();
		}
	}
}