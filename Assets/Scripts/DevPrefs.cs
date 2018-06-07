using UnityEngine;
using LunraGames.SpaceFarm;

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
			get { return GetBool(Keys.AutoApplyShaderGlobals, true); }
			set { SetBool(Keys.AutoApplyShaderGlobals, value); }
		}

		public static bool WindInEditMode
		{
			get { return GetBool(Keys.WindInEditMode, false); }
			set { SetBool(Keys.WindInEditMode, value); }
		}

		public static bool AutoApplySkybox
		{
			get { return GetBool(Keys.AutoApplySkybox, false); }
			set { SetBool(Keys.AutoApplySkybox, value); }
		}

		public static bool ApplyXButtonStyleInEditMode
		{
			get { return GetBool(Keys.ApplyXButtonStyleInEditMode, true); }
			set { SetBool(Keys.ApplyXButtonStyleInEditMode, value); }
		}

		// TODO: This should be somewhere else... if it doesn't exist in a LG plugin already.
		#region Utility
		static bool GetBool(string key, bool defaultValue)
		{
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
		}

		static void SetBool(string key, bool value)
		{
			PlayerPrefs.SetInt(key, value ? 1 : 0);
		}
		#endregion
	}
}
