namespace LunraGames.SubLight
{
	public static class DevPrefs
	{
		public static readonly DevPrefsBool AutoApplyShaderGlobals = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoApplyShaderGlobals", true);
		public static readonly DevPrefsBool WindInEditMode = new DevPrefsBool(ProjectConstants.PrefsPrefix + "WindInEditMode");
		public static readonly DevPrefsBool ApplyXButtonStyleInEditMode = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ApplyXButtonStyleInEditMode", true);
		public static readonly DevPrefsBool SkipMainMenuAnimations = new DevPrefsBool(ProjectConstants.PrefsPrefix + "SkipMainMenuAnimations");
		public static readonly DevPrefsBool HideMainMenu = new DevPrefsBool(ProjectConstants.PrefsPrefix + "HideMainMenu");
		public static readonly DevPrefsBool ShowHoloHelper = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ShowHoloHelper");
		public static readonly DevPrefsBool ShowUxHelper = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ShowUxHelper");
		public static readonly DevPrefsBool IgnoreGuiExitExceptions = new DevPrefsBool(ProjectConstants.PrefsPrefix + "IgnoreGuiExitExceptions");

		public static readonly DevPrefsBool ApplyTimeScaling = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ApplyTimeScaling");
		public static readonly DevPrefsFloat TimeScaling = new DevPrefsFloat(ProjectConstants.PrefsPrefix + "TimeScaling");
		public static readonly DevPrefsBool WipeGameSavesOnStart = new DevPrefsBool(ProjectConstants.PrefsPrefix + "WipeGameSavesOnStart");

		public static readonly DevPrefsString EncounterIdOverride = new DevPrefsString(ProjectConstants.PrefsPrefix + "EncounterIdOverride");
		public static readonly DevPrefsBool EncounterIdOverrideIgnore = new DevPrefsBool(ProjectConstants.PrefsPrefix + "EncounterIdOverrideIgnore");
		public static readonly DevPrefsEnum<EncounterTriggers> EncounterIdOverrideTrigger = new DevPrefsEnum<EncounterTriggers>(ProjectConstants.PrefsPrefix + "EncounterIdOverrideTrigger");
		public static bool EncounterIdOverrideActive
		{
			get
			{
				return !string.IsNullOrEmpty(EncounterIdOverride.Value) && !EncounterIdOverrideIgnore.Value && EncounterIdOverrideTrigger.Value != EncounterTriggers.Unknown;
			}
		}

		public static readonly DevPrefsEnum<AutoGameOptions> AutoGameOption = new DevPrefsEnum<AutoGameOptions>(ProjectConstants.PrefsPrefix + "AutoGameOption", AutoGameOptions.None);
		public static readonly DevPrefsBool AutoGameOptionRepeats = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoGameOptionRepeats");

		#region Create Game Overrides
		public static readonly DevPrefsToggle<DevPrefsInt, int> GameSeed = new DevPrefsToggle<DevPrefsInt, int>(new DevPrefsInt(ProjectConstants.PrefsPrefix + "GameSeed"));
		public static readonly DevPrefsToggle<DevPrefsInt, int> GalaxySeed = new DevPrefsToggle<DevPrefsInt, int>(new DevPrefsInt(ProjectConstants.PrefsPrefix + "GalaxySeed"));
		public static readonly DevPrefsToggle<DevPrefsString, string> GalaxyId = new DevPrefsToggle<DevPrefsString, string>(new DevPrefsString(ProjectConstants.PrefsPrefix + "GalaxyId"));
		public static readonly DevPrefsToggle<DevPrefsString, string> GamemodeId = new DevPrefsToggle<DevPrefsString, string>(new DevPrefsString(ProjectConstants.PrefsPrefix + "GamemodeId"));
		public static readonly DevPrefsToggle<DevPrefsEnum<ToolbarSelections>, ToolbarSelections> ToolbarSelection = new DevPrefsToggle<DevPrefsEnum<ToolbarSelections>, ToolbarSelections>(new DevPrefsEnum<ToolbarSelections>(ProjectConstants.PrefsPrefix + "ToolbarSelection"));
		#endregion

		#region Logging
		public static readonly DevPrefsBool LoggingInitialization = new DevPrefsBool(ProjectConstants.PrefsPrefix + "LoggingInitialization");
		public static readonly DevPrefsBool LoggingStateMachine = new DevPrefsBool(ProjectConstants.PrefsPrefix + "LoggingStateMachine");
		public static readonly DevPrefsBool LoggingAudio = new DevPrefsBool(ProjectConstants.PrefsPrefix + "LoggingAudio");
		#endregion
	}
}
