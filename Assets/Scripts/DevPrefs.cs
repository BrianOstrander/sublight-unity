namespace LunraGames.SubLight
{
	public static class DevPrefs
	{
		public static readonly DevPrefsBool AutoApplyShaderGlobals = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoApplyShaderGlobals", true);
		public static readonly DevPrefsBool WindInEditMode = new DevPrefsBool(ProjectConstants.PrefsPrefix + "WindInEditMode", false);
		public static readonly DevPrefsBool AutoApplySkybox = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoApplySkybox", false);
		public static readonly DevPrefsBool ApplyXButtonStyleInEditMode = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ApplyXButtonStyleInEditMode", true);
		public static readonly DevPrefsBool SkipExplanation = new DevPrefsBool(ProjectConstants.PrefsPrefix + "SkipExplanation", false);
		public static readonly DevPrefsBool ShowHoloHelper = new DevPrefsBool(ProjectConstants.PrefsPrefix + "ShowHoloHelper", false);

		public static readonly DevPrefsBool AutoNewGame = new DevPrefsBool(ProjectConstants.PrefsPrefix + "AutoNewGame", false);
		public static readonly DevPrefsBool WipeGameSavesOnStart = new DevPrefsBool(ProjectConstants.PrefsPrefix + "WipeGameSavesOnStart", false);
	}
}
