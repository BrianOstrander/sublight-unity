using System;
using System.IO;
using System.Linq;

using LunraGamesEditor;

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
		const string KeyPrefix = "LG_SF_DeveloperSettings_";

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

			GUILayout.BeginHorizontal();
			{
				if (BuildPreferences.Instance == null) EditorGUILayout.HelpBox("The scriptable object for build preferences cannot be found", MessageType.Error);
				else
				{
					if (GUILayout.Button("See build preferences")) Selection.activeObject = BuildPreferences.Instance;
				}

				if (DefaultShaderGlobals.Instance == null) EditorGUILayout.HelpBox("The scriptable object for default shader globals cannot be found", MessageType.Error);
				else
				{
					if (GUILayout.Button("See all shader globals")) Selection.activeObject = DefaultShaderGlobals.Instance;
				}
			}
			GUILayout.EndHorizontal();

			DevPrefs.WindInEditMode.Value = GUILayout.Toggle(DevPrefs.WindInEditMode, "Wind In Edit Mode");
			DevPrefs.AutoApplySkybox.Value = GUILayout.Toggle(DevPrefs.AutoApplySkybox, "Auto Apply Skybox");
			DevPrefs.ApplyXButtonStyleInEditMode.Value = GUILayout.Toggle(DevPrefs.ApplyXButtonStyleInEditMode, "Apply XButton Styles In Edit Mode");
			#endregion

			#region Utility
			GUILayout.Label("Utility", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical();
				{
					DevPrefs.SkipExplanation.Value = GUILayout.Toggle(DevPrefs.SkipExplanation, "Skip Explanation");
					var isWiping = DevPrefs.WipeGameSavesOnStart.Value;
					if (isWiping) EditorGUILayoutExtensions.PushColor(Color.red);
					DevPrefs.WipeGameSavesOnStart.Value = GUILayout.Toggle(DevPrefs.WipeGameSavesOnStart, "Wipe Game Saves on Start");
					if (isWiping) EditorGUILayoutExtensions.PopColor();
				}
				GUILayout.EndVertical();
				DevPrefs.AutoNewGame.Value = GUILayout.Toggle(DevPrefs.AutoNewGame, "Auto New Game");
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushEnabled(IsInGameState);
				if (GUILayout.Button("Copy Game JSON to Clipboard"))
				{
					OnGameSaved(new SaveRequest(SaveRequest.States.Complete));
				}
				if (GUILayout.Button("Save Then Copy JSON to Clipboard"))
				{
					App.Callbacks.SaveRequest += OnGameSaved;
					App.Callbacks.SaveRequest(SaveRequest.Save());
				}
				EditorGUILayoutExtensions.PopEnabled();
			}
			GUILayout.EndHorizontal();
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

		void OnGameSaved(SaveRequest result)
		{
			if (result.State != SaveRequest.States.Complete) return;
			App.Callbacks.SaveRequest -= OnGameSaved;
			try
			{
				EditorGUIUtility.systemCopyBuffer = File.ReadAllText((App.SM.CurrentHandler as GameState).Payload.Game.Path);
				Debug.Log("Game JSON Copied to Clipboard");
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		bool IsInGameState
		{
			get
			{
				return Application.isPlaying && 
					App.HasInstance && 
					App.SM != null && 
					App.SM.Is(StateMachine.States.Game, StateMachine.Events.Idle);
			}
		}
	}
}