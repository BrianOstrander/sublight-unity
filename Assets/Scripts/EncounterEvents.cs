using System;

namespace LunraGames.SubLight
{
	public static class EncounterEvents
	{
		public enum Types
		{
			Unknown = 0,
			Custom = 10,
			DebugLog = 20,
			ToolbarSelection = 30
		}

		public static class DebugLog
		{
			public enum Severities
			{
				Unknown = 0,
				Normal = 10,
				Warning = 20,
				Error = 30,
				Break = 40
			}

			public static class EnumKeys
			{
				public const string Severity = "Severity";
			}

			public static class StringKeys
			{
				public const string Message = "Message";
			}
		}

		public static class ToolbarSelection
		{
			public static class EnumKeys
			{
				public const string Selection = "Selection";
			}
		}
	}
}