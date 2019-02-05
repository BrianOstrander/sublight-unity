namespace LunraGames.SubLight
{
	public static class DevPrefs
	{
		public static readonly DevPrefsBool AutoApplyShaderGlobals = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoApplyShaderGlobals", true);
		public static readonly DevPrefsBool WindInEditMode = new DevPrefsBool(ProjectConstants.PrefsPrefix + "WindInEditMode");
		public static readonly DevPrefsBool ApplyXButtonStyleInEditMode = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ApplyXButtonStyleInEditMode", true);
		public static readonly DevPrefsBool SkipExplanation = new DevPrefsBool(ProjectConstants.PrefsPrefix + "SkipExplanation");
		public static readonly DevPrefsBool SkipMainMenuAnimations = new DevPrefsBool(ProjectConstants.PrefsPrefix + "SkipMainMenuAnimations");
		public static readonly DevPrefsBool ShowHoloHelper = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ShowHoloHelper");
		public static readonly DevPrefsBool ShowUxHelper = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ShowUxHelper");
		public static readonly DevPrefsBool IgnoreGuiExitExceptions = new DevPrefsBool(ProjectConstants.PrefsPrefix + "IgnoreGuiExitExceptions");

		public static readonly DevPrefsBool ApplyTimeScaling = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ApplyTimeScaling");
		public static readonly DevPrefsFloat TimeScaling = new DevPrefsFloat(ProjectConstants.PrefsPrefix + "TimeScaling");
		public static readonly DevPrefsBool AutoNewGame = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoNewGame");
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

		public static readonly DevPrefsInt GameSeed = new DevPrefsInt(ProjectConstants.PrefsPrefix + "GameSeed");
		public static readonly DevPrefsInt GalaxySeed = new DevPrefsInt(ProjectConstants.PrefsPrefix + "GalaxySeed");
		public static readonly DevPrefsString GalaxyId = new DevPrefsString(ProjectConstants.PrefsPrefix + "GalaxyId");
		public static readonly DevPrefsEnum<ToolbarSelections> ToolbarSelection = new DevPrefsEnum<ToolbarSelections>(ProjectConstants.PrefsPrefix + "ToolbarSelection");

		/// <summary>
		/// Gets a created game block using the dev preferences.
		/// </summary>
		/// <value>The dev create game.</value>
		public static CreateGameBlock DevCreateGame
		{
			get
			{
				return new CreateGameBlock
				{
					GameSeed = GameSeed.Value,
					GalaxySeed = GalaxySeed.Value,
					GalaxyId = GalaxyId.Value,
					ToolbarSelection = ToolbarSelection.Value
				};
			}
		}
	}
}
