using System;

namespace LunraGames.SubLight
{
	public static class EncounterEvents
	{
		public enum Types
		{
			Unknown = 0,
			Custom = 10,
			Debug = 20,
			ToolbarSelection = 30,
			DumpKeyValues = 40,
			GameComplete = 50,
			PopTriggers = 60
		}

		public static class Debug
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
				public const string Severity = "severity";
			}

			public static class StringKeys
			{
				public const string Message = "message";
			}
		}

		public static class ToolbarSelection
		{
			public enum LockStates
			{
				Unknown = 0,
				Lock = 10,
				UnLock = 20
			}

			public static class EnumKeys
			{
				public const string Selection = "selection";
				public const string LockState = "lock_state";
			}
		}

		public static class DumpKeyValues
		{
			public static class EnumKeys
			{
				public const string Target = "target";
			}
		}

		public static class GameComplete
		{
			public enum Conditions
			{
				Unknown = 0,
				Success = 10,
				Failure = 20
			}

			public static class EnumKeys
			{
				public const string Condition = "condition";
			}

			public static class StringKeys
			{
				public const string Title = "title";
				public const string Message = "message";
			}
		}

		public static class PopTriggers
		{
			public static class BooleanKeys
			{
				public const string PopTransitComplete = "pop_transit_complete";
				public const string PopResourceRequest = "pop_resource_request";
				public const string PopSystemIdle = "pop_system_idle";
			}
		}
	}
}