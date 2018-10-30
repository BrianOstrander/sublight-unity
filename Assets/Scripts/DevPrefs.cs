namespace LunraGames.SubLight
{
	public static class DevPrefs
	{
		public static readonly DevPrefsBool AutoApplyShaderGlobals = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoApplyShaderGlobals", true);
		public static readonly DevPrefsBool WindInEditMode = new DevPrefsBool(ProjectConstants.PrefsPrefix + "WindInEditMode");
		public static readonly DevPrefsBool AutoApplySkybox = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoApplySkybox");
		public static readonly DevPrefsBool ApplyXButtonStyleInEditMode = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ApplyXButtonStyleInEditMode", true);
		public static readonly DevPrefsBool SkipExplanation = new DevPrefsBool(ProjectConstants.PrefsPrefix + "SkipExplanation");
		public static readonly DevPrefsBool SkipMainMenuAnimations = new DevPrefsBool(ProjectConstants.PrefsPrefix + "SkipMainMenuAnimations");
		public static readonly DevPrefsBool ShowHoloHelper = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ShowHoloHelper");
		public static readonly DevPrefsBool ShowUxHelper = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ShowUxHelper");

		public static readonly DevPrefsBool AutoNewGame = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoNewGame");
		public static readonly DevPrefsBool WipeGameSavesOnStart = new DevPrefsBool(ProjectConstants.PrefsPrefix + "WipeGameSavesOnStart");

		public static readonly DevPrefsInt GameSeed = new DevPrefsInt(ProjectConstants.PrefsPrefix + "GameSeed");
		public static readonly DevPrefsInt GalaxySeed = new DevPrefsInt(ProjectConstants.PrefsPrefix + "GalaxySeed");
		public static readonly DevPrefsString GalaxyId = new DevPrefsString(ProjectConstants.PrefsPrefix + "GalaxyId");

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
					GalaxyId = GalaxyId.Value
				};
			}
		}
	}
}
