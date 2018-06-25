using UnityEditor;
using UnityEngine;
using LunraGamesEditor.Singletonnes;

namespace LunraGames.SpaceFarm
{
	[CustomEditor(typeof(DefaultShaderGlobals), true)]
	public class DefaultShaderGlobalsEditor : ScriptableSingletonEditor
	{
		protected override void OnInspectorGUIExtended()
		{
			var typedTarget = target as DefaultShaderGlobals;

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = !DevPrefs.AutoApplyShaderGlobals;
				if (GUILayout.Button("Apply Now")) typedTarget.Apply();
				GUI.enabled = true;
				DevPrefs.AutoApplyShaderGlobals = GUILayout.Toggle(DevPrefs.AutoApplyShaderGlobals, "Auto Apply", "Button");
			}
			GUILayout.EndHorizontal();

			if (DevPrefs.AutoApplyShaderGlobals)
			{
				typedTarget.Apply();
			}

			GUILayout.Space(16f);
			DrawDefaultInspector();

			EditorUtility.SetDirty(target);
		}
	}
}