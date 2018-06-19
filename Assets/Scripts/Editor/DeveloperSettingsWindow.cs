using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public interface ILocalDeveloperSettingsWindow
	{
		void OnLocalGUI();
	}

#pragma warning disable RECS0001 // Class is declared partial but has only one part
	public partial class DeveloperSettingsWindow : EditorWindow
#pragma warning restore RECS0001 // Class is declared partial but has only one part
	{
		[MenuItem("Window/Lunra Games/Development Settings")]
		static void Initialize()
		{
			GetWindow(typeof(DeveloperSettingsWindow), false, "Developer Settings").Show();
		}


		void OnGUI()
		{
			GUILayout.Label("Note: These values are local to your machine only.");

			#region Editor Globals
			GUILayout.Label("Globals", EditorStyles.boldLabel);
			if (DefaultShaderGlobals.Instance == null) EditorGUILayout.HelpBox("The scriptable object for default shader globals cannot be found", MessageType.Error);
			else
			{
				if (GUILayout.Button("See all shader globals")) Selection.activeObject = DefaultShaderGlobals.Instance;
			}
			DevPrefs.WindInEditMode = GUILayout.Toggle(DevPrefs.WindInEditMode, "Wind In Edit Mode");
			DevPrefs.AutoApplySkybox = GUILayout.Toggle(DevPrefs.AutoApplySkybox, "Auto Apply Skybox");
			DevPrefs.ApplyXButtonStyleInEditMode = GUILayout.Toggle(DevPrefs.ApplyXButtonStyleInEditMode, "Apply XButton Styles In Edit Mode");
			#endregion

			#region Interface
			GUILayout.Label("Interface", EditorStyles.boldLabel);
			DevPrefs.SkipExplanation = GUILayout.Toggle(DevPrefs.SkipExplanation, "Skip Explanation");
   			#endregion

			#region Logging
			GUILayout.Label("Logging", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			{
				foreach (var logType in Enum.GetValues(typeof(LogTypes)).Cast<LogTypes>())
				{
					EditorLogService.SetLogActive(logType, GUILayout.Toggle(EditorLogService.GetLogActive(logType), logType.ToString(), "Button"));
				}
			}
			GUILayout.EndHorizontal();
			#endregion

			Space();

			if (this is ILocalDeveloperSettingsWindow)
			{
				GUILayout.Label("Local developer settings", EditorStyles.boldLabel);
				try 
				{
					var localWindow = (this as ILocalDeveloperSettingsWindow);
					localWindow.OnLocalGUI(); 
				}
				catch (Exception e)
				{
					GUILayout.BeginHorizontal();
					{
						EditorGUILayout.HelpBox(e.Message, MessageType.Error);
						if (GUILayout.Button("Print Exception")) Debug.LogException(e);
					}
					GUILayout.EndHorizontal();
				}
			}
			else GUILayout.Label("No local developer settings detected. Ask Brian how to create them!", EditorStyles.boldLabel);
		}

		void Space()
		{
			GUILayout.Space(16f);
		}
	}
}