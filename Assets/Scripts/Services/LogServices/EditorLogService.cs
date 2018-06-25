#if UNITY_EDITOR
using UnityEditor;

namespace LunraGames.SpaceFarm
{
	public class EditorLogService : LogService
	{
		public static string GetLogKey(LogTypes logType) { return ProjectConstants.PrefsPrefix + "LOGGING_" + logType; }

		public static bool GetLogActive(LogTypes logType) { return EditorPrefs.GetBool(GetLogKey(logType), true); }

		public static void SetLogActive(LogTypes logType, bool isActive) { EditorPrefs.SetBool(GetLogKey(logType), isActive); }

		public override bool GetActive(LogTypes logType)
		{
			return GetLogActive(logType);
		}

		public override void SetActive(LogTypes logType, bool isActive)
		{
			SetLogActive(logType, isActive);
		}
	}
}
#endif