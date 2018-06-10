#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LunraGames.SpaceFarm
{
	public static class DevPrefs
	{
		static class Keys
		{
			public const string AutoApplyShaderGlobals = ProjectConstants.PrefsPrefix + "AutoApplyShaderGlobals";
			public const string WindInEditMode = ProjectConstants.PrefsPrefix + "WindInEditMode";
			public const string AutoApplySkybox = ProjectConstants.PrefsPrefix + "AutoApplySkybox";
			public const string ApplyXButtonStyleInEditMode = ProjectConstants.PrefsPrefix + "ApplyXButtonStyleInEditMode";
		}

		public static bool AutoApplyShaderGlobals
		{
#if UNITY_EDITOR
			get { return EditorPrefs.GetBool(Keys.AutoApplyShaderGlobals, true); }
			set { EditorPrefs.SetBool(Keys.AutoApplyShaderGlobals, value); }
#else
			get; set;
#endif
		}

		public static bool WindInEditMode
		{
#if UNITY_EDITOR
			get { return EditorPrefs.GetBool(Keys.WindInEditMode, false); }
			set { EditorPrefs.SetBool(Keys.WindInEditMode, value); }
#else
			get; set;
#endif
		}

		public static bool AutoApplySkybox
		{
#if UNITY_EDITOR
			get { return EditorPrefs.GetBool(Keys.AutoApplySkybox, false); }
			set { EditorPrefs.SetBool(Keys.AutoApplySkybox, value); }
#else
			get; set;
#endif
		}

		public static bool ApplyXButtonStyleInEditMode
		{
#if UNITY_EDITOR
			get { return EditorPrefs.GetBool(Keys.ApplyXButtonStyleInEditMode, true); }
			set { EditorPrefs.SetBool(Keys.ApplyXButtonStyleInEditMode, value); }
#else
			get; set;
#endif
		}

	}
}
