using System;
using System.IO;
using System.Linq;

using LunraGamesEditor;

using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;

using LunraGames.SubLight.Models;

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

        [SerializeField]
        GameObject stagedPrefab;
        [SerializeField]
        bool stagedPrefabEnabled;

		public DeveloperSettingsWindow()
		{
			currentTab = new DevPrefsInt(KeyPrefix + "CurrentTab");

            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
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
                var wasStagedPrefabEnabled = !stagedPrefabEnabled;
                if (wasStagedPrefabEnabled) EditorGUILayoutExtensions.PushColor(Color.grey);
                {
                    GUILayout.Label("Staged", GUILayout.ExpandWidth(false));
                    stagedPrefab = (GameObject)EditorGUILayout.ObjectField(stagedPrefab, typeof(GameObject), false);
                    stagedPrefabEnabled = EditorGUILayout.Toggle(stagedPrefabEnabled, GUILayout.Width(14f));
                    EditorGUILayoutExtensions.PushEnabled(stagedPrefab != null);
                    {
                        if (GUILayout.Button("Load", GUILayout.ExpandWidth(false))) OnLoadStagedPrefab();
                    }
                    EditorGUILayoutExtensions.PopEnabled();
                }
                if (wasStagedPrefabEnabled) EditorGUILayoutExtensions.PopColor();
            }
            GUILayout.EndHorizontal();

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
				var loggingOptions = new GUILayoutOption[] { GUILayout.ExpandWidth(true) };
				DevPrefs.LoggingInitialization.Value = EditorGUILayoutExtensions.ToggleButtonCompact("Initialization", DevPrefs.LoggingInitialization.Value, options: loggingOptions);
				DevPrefs.LoggingStateMachine.Value = EditorGUILayoutExtensions.ToggleButtonCompact("StateMachine", DevPrefs.LoggingStateMachine.Value, options: loggingOptions);
				DevPrefs.LoggingAudio.Value = EditorGUILayoutExtensions.ToggleButtonCompact("Audio", DevPrefs.LoggingAudio.Value, options: loggingOptions);
			}
			GUILayout.EndHorizontal();
			#endregion
		}

		void OnGameTab()
		{
			var disabledColor = Color.white.NewV(0.7f);

			GUILayout.BeginHorizontal();
			{
				if (!DevPrefs.ApplyTimeScaling.Value) EditorGUILayoutExtensions.PushColor(disabledColor);
				{
					DevPrefs.TimeScaling.Value = EditorGUILayout.Slider("Time Scale", DevPrefs.TimeScaling.Value, 0f, 4f);
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
					DevPrefs.HideMainMenu.Value = GUILayout.Toggle(DevPrefs.HideMainMenu, "Hide Main Menu");
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

			EditorGUILayoutExtensions.PushIndent();
			{
				DevPrefs.AutoGameOptionRepeats.Value = EditorGUILayout.Toggle(new GUIContent("Repeat on Main Menu", "If enabled, everytime the game returns to the main menu this logic will run."), DevPrefs.AutoGameOptionRepeats);
				switch (DevPrefs.AutoGameOption.Value)
				{
					case AutoGameOptions.None: break;
					case AutoGameOptions.NewGame:
					case AutoGameOptions.OverrideGame:
						EditorGUILayoutDevPrefsToggle.Field(DevPrefs.GameSeed, p => p.Value = EditorGUILayout.IntField("Game Seed", p));
						EditorGUILayoutDevPrefsToggle.Field(DevPrefs.GalaxySeed, p => p.Value = EditorGUILayout.IntField("Galaxy Seed", p));
						EditorGUILayoutDevPrefsToggle.Field(DevPrefs.GalaxyId, p => p.Value = EditorGUILayout.TextField("Galaxy Id", p));
						EditorGUILayoutDevPrefsToggle.Field(DevPrefs.GamemodeId, p => p.Value = EditorGUILayout.TextField("Gamemode Id", p));
						EditorGUILayoutDevPrefsToggle.Field(DevPrefs.ToolbarSelection, p => p.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(new GUIContent("Toolbar Selection"), "- Select an Override -", DevPrefs.ToolbarSelection.Value));
						break;
					case AutoGameOptions.ContinueGame:
						GUILayout.Label("todo");
						break;
					default:
						EditorGUILayout.HelpBox("Unrecognized AutoGameOption: " + DevPrefs.AutoGameOption.Value, MessageType.Error);
						break;
				}
			}
			EditorGUILayoutExtensions.PopIndent();

			GUILayout.BeginHorizontal();
			{
				var encounterNotOverriding = !DevPrefs.EncounterIdOverrideActive;
				if (encounterNotOverriding) EditorGUILayoutExtensions.PushColor(disabledColor);
				{
					EditorGUILayout.PrefixLabel(new GUIContent("Encounter Overriding", "If a valid trigger and Encounter Id are selected, the specified Encounter will run when the appropriate trigger occurs."));
					DevPrefs.EncounterIdOverrideTrigger.Value = EditorGUILayoutExtensions.HelpfulEnumPopup(
						GUIContent.none,
						"- Select a Trigger -",
						DevPrefs.EncounterIdOverrideTrigger.Value,
						guiOptions: GUILayout.Width(120f)
					);
					DevPrefs.EncounterIdOverride.Value = EditorGUILayout.TextField(DevPrefs.EncounterIdOverride.Value);
				}
				if (encounterNotOverriding) EditorGUILayoutExtensions.PopColor();
				DevPrefs.EncounterIdOverrideIgnore.Value = !EditorGUILayout.Toggle(!DevPrefs.EncounterIdOverrideIgnore.Value, GUILayout.Width(18f));

				EditorGUILayoutExtensions.PushEnabled(IsInGameState);
				{
					if (GUILayout.Button(new GUIContent("Start", "Starts this encounter if none are currently active."), EditorStyles.miniButton, GUILayout.Width(64f)))
					{
						StartEncounter();
					}
					//if (GUILayout.Button(new GUIContent("Stop", "Stops an encounter if one is playing."), EditorStyles.miniButtonRight, GUILayout.Width(64f)))
					//{

					//}
				}
				EditorGUILayoutExtensions.PopEnabled();
			}
			GUILayout.EndHorizontal();
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

        #region Events
        void OnPlaymodeStateChanged(PlayModeStateChange playmode)
        {
            switch (playmode)
            {
                case PlayModeStateChange.EnteredEditMode:
                    if (stagedPrefabEnabled && stagedPrefab != null) OnLoadStagedPrefab();
                    break;
            }
        }

        void OnLoadStagedPrefab()
        {
            AssetDatabase.OpenAsset(stagedPrefab);
        }
		#endregion

		#region Shared
		public static bool IsInGameState
		{
			get
			{
				return Application.isPlaying &&
					App.HasInstance &&
					App.SM != null &&
					App.SM.Is(StateMachine.States.Game, StateMachine.Events.Idle);
			}
		}

		public static void StartEncounter()
		{
			var gamePayload = (App.SM.CurrentHandler as GameState).Payload;

			Action onStartEncounter = () =>
			{
				App.M.Load<EncounterInfoModel>(
					DevPrefs.EncounterIdOverride.Value,
					encounterResult =>
					{
						if (encounterResult.Status != RequestStatus.Success)
						{
							Debug.LogError("Loading override encounter returned status: " + encounterResult.Status + " and error: " + encounterResult.Error);
							return;
						}

						if (encounterResult.TypedModel == null) Debug.LogError("Unable to push encounter, could not find one with a matching id");
						else
						{
							EditorUtilityExtensions.GetGameWindow().Focus();
							App.Callbacks.EncounterRequest(
								EncounterRequest.Request(
									gamePayload.Game,
									encounterResult.TypedModel,
									DevPrefs.EncounterIdOverrideTrigger.Value
								)
							);
						}
					}
				);
			};

			if (gamePayload.Game.EncounterResume.Value.CanResume)
			{
				Debug.LogWarning("Interrupting active encounter to start a new one... This behaviour is very buggy right now!");
				//Debug.LogError("Cannot start an encounter while one is active.");
				App.Callbacks.EncounterRequest(EncounterRequest.Controls(false, true));
				App.Heartbeat.Wait(onStartEncounter, 0.25f);
				EditorUtilityExtensions.GetGameWindow().Focus();
			}
			else onStartEncounter();
		}
		#endregion
	}
}