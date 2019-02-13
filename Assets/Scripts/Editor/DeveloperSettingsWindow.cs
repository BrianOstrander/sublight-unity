using System;
using System.IO;
using System.Linq;

using LunraGamesEditor;

using UnityEditor;
using UnityEngine;

namespace LunraGames.SubLight
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

		static string[] TabNames = {
			"General",
			"Game",
			"Local"
		};

		DevPrefsInt currentTab;

		public DeveloperSettingsWindow()
		{
			currentTab = new DevPrefsInt(KeyPrefix + "CurrentTab");
		}

		[MenuItem("Window/Lunra Games/Development Settings")]
		static void Initialize()
		{
			GetWindow(typeof(DeveloperSettingsWindow), false, "Developer Settings").Show();
		}

		void OnGUI()
		{
			GUILayout.Label("Note: These values are local to your machine only.");
			currentTab.Value = GUILayout.Toolbar(currentTab.Value, TabNames);

			switch (currentTab.Value)
			{
				case 0: OnGeneralTab(); break;
				case 1: OnGameTab(); break;
				case 2: OnLocalTab(); break;
				default:
					EditorGUILayout.HelpBox("Unrecognized tab value: " + currentTab.Value, MessageType.Error);
					if (GUILayout.Button("Reset")) currentTab.Value = 0;
					break;
			}
		}

		void OnGeneralTab()
		{
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
			DevPrefs.ApplyXButtonStyleInEditMode.Value = GUILayout.Toggle(DevPrefs.ApplyXButtonStyleInEditMode, "Apply XButton Styles In Edit Mode");
			#endregion

			#region Utility
			GUILayout.Label("Utility", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical();
				{
					DevPrefs.ShowHoloHelper.Value = GUILayout.Toggle(DevPrefs.ShowHoloHelper, "Show Holo Helper");
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						DevPrefs.ShowUxHelper.Value = GUILayout.Toggle(DevPrefs.ShowUxHelper, "Show UX Helper");
						if (check.changed) UxHelper.RunUpdate();
					}
				}
				GUILayout.EndVertical();

				GUILayout.BeginVertical();
				{
					// second column of controlls here
					DevPrefs.IgnoreGuiExitExceptions.Value = GUILayout.Toggle(DevPrefs.IgnoreGuiExitExceptions, "Ignore GuiExit Exceptions");
				}
				GUILayout.EndVertical();

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				EditorGUILayoutExtensions.PushEnabled(IsInGameState);
				if (GUILayout.Button("Copy Game JSON to Clipboard"))
				{
					OnGameSaved(SaveRequest.Success(SaveRequest.Request()));
				}
				if (GUILayout.Button("Save Then Copy JSON to Clipboard"))
				{
					App.Callbacks.SaveRequest += OnGameSaved;
					App.Callbacks.SaveRequest(SaveRequest.Request());
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
		}

		void OnGameTab()
		{
			GUILayout.BeginHorizontal();
			{
				if (!DevPrefs.ApplyTimeScaling.Value) EditorGUILayoutExtensions.PushColor(Color.white.NewV(0.7f));
				{
					DevPrefs.TimeScaling.Value = EditorGUILayout.Slider("Time Scale", DevPrefs.TimeScaling.Value, 0f, 1f);
				}
				if (!DevPrefs.ApplyTimeScaling.Value) EditorGUILayoutExtensions.PopColor();

				DevPrefs.ApplyTimeScaling.Value = EditorGUILayout.Toggle(DevPrefs.ApplyTimeScaling.Value, GUILayout.Width(12f));
				if (DevPrefs.ApplyTimeScaling.Value && Application.isPlaying) Time.timeScale = DevPrefs.TimeScaling.Value;

				EditorGUILayoutExtensions.PushEnabled(Application.isPlaying && !Mathf.Approximately(DevPrefs.TimeScaling.Value, Time.timeScale));
				{
					if (GUILayout.Button("Apply Once", EditorStyles.miniButtonLeft)) Time.timeScale = DevPrefs.TimeScaling;
				}
				EditorGUILayoutExtensions.PopEnabled();

				if (GUILayout.Button("Reset", EditorStyles.miniButtonRight))
				{
					DevPrefs.TimeScaling.Value = 1f;
					Time.timeScale = 1f;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical();
				{
					DevPrefs.SkipMainMenuAnimations.Value = GUILayout.Toggle(DevPrefs.SkipMainMenuAnimations, "Skip Main Menu Animations");
				}
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				{
					var isWiping = DevPrefs.WipeGameSavesOnStart.Value;
					if (isWiping) EditorGUILayoutExtensions.PushColor(Color.red);
					DevPrefs.WipeGameSavesOnStart.Value = GUILayout.Toggle(DevPrefs.WipeGameSavesOnStart, "Wipe Game Saves on Start");
					if (isWiping) EditorGUILayoutExtensions.PopColor();
				}
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			DevPrefs.AutoGameOption.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(
				new GUIContent("Auto Game Behaviour"),
				"- Select Auto Game Behaviour -",
				DevPrefs.AutoGameOption.Value,
				EnumExtensions.GetValues(AutoGameOptions.Unknown)
			);

			switch (DevPrefs.AutoGameOption.Value)
			{
				case AutoGameOptions.None: break;
				case AutoGameOptions.NewGame:
					EditorGUILayoutExtensions.PushIndent();
					{
						DevPrefs.GameSeed.Value = EditorGUILayout.IntField("Game Seed", DevPrefs.GameSeed.Value);
						DevPrefs.GalaxySeed.Value = EditorGUILayout.IntField("Galaxy Seed", DevPrefs.GalaxySeed.Value);
						DevPrefs.GalaxyId.Value = EditorGUILayout.TextField("Galaxy Id", DevPrefs.GalaxyId.Value);
						DevPrefs.ToolbarSelection.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(new GUIContent("Toolbar Selection"), "- Select an Override -", DevPrefs.ToolbarSelection.Value);
					}
					EditorGUILayoutExtensions.PopIndent();
					break;
				case AutoGameOptions.ContinueGame:
					GUILayout.Label("todo");
					break;
				default:
					EditorGUILayout.HelpBox("Unrecognized AutoGameOption: " + DevPrefs.AutoGameOption.Value, MessageType.Error);
					break;
			}

			var encounterNotOverriding = !DevPrefs.EncounterIdOverrideActive;
			if (encounterNotOverriding) EditorGUILayoutExtensions.PushColor(Color.gray);
			{
				GUILayout.BeginHorizontal();
				{
					EditorGUILayout.PrefixLabel(new GUIContent("Encounter Overriding", "If a valid trigger and Encounter Id are selected, the specified Encounter will run when the appropriate trigger occurs."));
					DevPrefs.EncounterIdOverrideTrigger.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(
						GUIContent.none,
						"- Select a Trigger -",
						DevPrefs.EncounterIdOverrideTrigger.Value,
						guiOptions: GUILayout.Width(120f)
					);
					DevPrefs.EncounterIdOverride.Value = EditorGUILayout.TextField(DevPrefs.EncounterIdOverride.Value);
					DevPrefs.EncounterIdOverrideIgnore.Value = !EditorGUILayout.Toggle(!DevPrefs.EncounterIdOverrideIgnore.Value, GUILayout.Width(18f));
				}
				GUILayout.EndHorizontal();
			}
			if (encounterNotOverriding) EditorGUILayoutExtensions.PopColor();
		}

		void OnLocalTab()
		{
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