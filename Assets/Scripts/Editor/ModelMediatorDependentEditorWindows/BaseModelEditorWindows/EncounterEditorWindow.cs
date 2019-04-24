using System;

using UnityEngine;

using UnityEditor;

using LunraGamesEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class EncounterEditorWindow : BaseModelEditorWindow<EncounterEditorWindow, EncounterInfoModel>
	{
		[MenuItem("Window/SubLight/Encounter Editor")]
		static void Initialize() { OnInitialize("Encounter Editor"); }

		EditorPrefsEnum<AutoGameOptions> baseRunAutoGame;
		EditorPrefsEnum<EncounterTriggers> baseRunEncounterTrigger;

		public EncounterEditorWindow() : base("LG_SL_EncounterEditor_", "Encounter")
		{
			var currPrefix = KeyPrefix + "Base";

			baseRunAutoGame = new EditorPrefsEnum<AutoGameOptions>(currPrefix + "RunAutoGame", AutoGameOptions.NewGame);
			baseRunEncounterTrigger = new EditorPrefsEnum<EncounterTriggers>(currPrefix + "RunEncounterTrigger", EncounterTriggers.Load);

			SettingsGui += BaseSettingsGui;

			GeneralConstruct();
			LogsConstruct();
		}

		void BaseSettingsGui()
		{
			GUILayout.Label("Base", EditorStyles.boldLabel);

			baseRunAutoGame.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				new GUIContent("Run Auto Game", "Determines how a new game is created when Run is selected."),
				"- Select Auto Game -",
				baseRunAutoGame.Value,
				Color.red,
				EnumExtensions.GetValues(AutoGameOptions.None)
			);

			baseRunEncounterTrigger.Value = EditorGUILayoutExtensions.HelpfulEnumPopupValidation(
				new GUIContent("Run Trigger", "Determines how an override behaviour is triggered upon running the game. Use \"Load\" to trigger right away."),
				"- Select Trigger -",
				baseRunEncounterTrigger.Value,
				Color.red,
				EnumExtensions.GetValues(EncounterTriggers.None)
			);
		}

		#region Model Overrides
		protected override EncounterInfoModel CreateModel(string name)
		{
			var model = base.CreateModel(name);

			model.RandomWeightMultiplier.Value = 1f;
			model.RandomAppearance.Value = 1f;
			model.Trigger.Value = EncounterTriggers.TransitComplete;

			return model;
		}

		protected override void AssignModelName(EncounterInfoModel model, string name)
		{
			model.Name.Value = name;
		}
		#endregion

		protected override void OnDrawPreSettings(EncounterInfoModel model)
		{
			EditorGUILayoutExtensions.PushEnabled(model != null);
			{
				if (DeveloperSettingsWindow.IsInGameState)
				{
					if (GUILayout.Button("Run", EditorStyles.toolbarButton, GUILayout.Width(ToolbarWidth)))
					{
						SetEncounterOverrides(model);

						DeveloperSettingsWindow.StartEncounter();
					}
				}
				else
				{
					EditorGUILayoutExtensions.PushEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
					{
						if (GUILayout.Button("Run", EditorStyles.toolbarButton, GUILayout.Width(ToolbarWidth)))
						{
							SetEncounterOverrides(model);

							EditorApplication.EnterPlaymode();
						}
					}
					EditorGUILayoutExtensions.PopEnabled();
				}
			}
			EditorGUILayoutExtensions.PopEnabled();
		}

		void SetEncounterOverrides(EncounterInfoModel model)
		{
			DevPrefs.AutoGameOption.Value = baseRunAutoGame.Value;
			DevPrefs.EncounterIdOverride.Value = model.Id.Value;
			DevPrefs.EncounterIdOverrideIgnore.Value = false;
			DevPrefs.EncounterIdOverrideTrigger.Value = baseRunEncounterTrigger.Value;
		}

		protected override bool CanEditDuringPlaymode()
		{
			return true;
		}
	}
}