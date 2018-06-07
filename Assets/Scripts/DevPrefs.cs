using PlayerPrefs = LunraGames.PlayerPrefsExtensions;

namespace LunraGames.SpaceFarm
{
	public static class DevPrefs
	{
		static class Keys
		{
			const string Prefix = "LG_SF_";
			public const string AutoApplyShaderGlobals = Prefix + "AutoApplyShaderGlobals";
			public const string WindInEditMode = Prefix + "WindInEditMode";
			public const string AutoApplySkybox = Prefix + "AutoApplySkybox";
			public const string ApplyXButtonStyleInEditMode = Prefix + "ApplyXButtonStyleInEditMode";
		}

		public static bool AutoApplyShaderGlobals
		{
			get { return PlayerPrefs.GetBool(Keys.AutoApplyShaderGlobals, true); }
			set { PlayerPrefs.SetBool(Keys.AutoApplyShaderGlobals, value); }
		}

		public static bool WindInEditMode
		{
			get { return PlayerPrefs.GetBool(Keys.WindInEditMode, false); }
			set { PlayerPrefs.SetBool(Keys.WindInEditMode, value); }
		}

		public static bool AutoApplySkybox
		{
			get { return PlayerPrefs.GetBool(Keys.AutoApplySkybox, false); }
			set { PlayerPrefs.SetBool(Keys.AutoApplySkybox, value); }
		}

		public static bool ApplyXButtonStyleInEditMode
		{
			get { return PlayerPrefs.GetBool(Keys.ApplyXButtonStyleInEditMode, true); }
			set { PlayerPrefs.SetBool(Keys.ApplyXButtonStyleInEditMode, value); }
		}
	}
}
