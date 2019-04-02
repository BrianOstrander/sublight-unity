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
			TriggerQueue = 60,
			Delay = 70
		}

		public static class Custom
		{
			public static class StringKeys
			{
				public const string CustomEventName = "custom_event_name";
			}
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
				public const string IconOverride = "icon_override";
			}

			public static class StringKeys
			{
				public const string Title = "title";
				public const string Header = "header";
				public const string Body = "body";
			}
		}

		public static class TriggerQueue
		{
			public const int PushDisabled = 0;

			static string TriggerNormalized(EncounterTriggers trigger) { return Enum.GetName(typeof(EncounterTriggers), trigger).ToLower(); }

			public static class BooleanKeys
			{
				public static string PopTrigger(EncounterTriggers trigger)
				{
					return "pop_" + TriggerNormalized(trigger);
				}
			}

			public static class IntegerKeys
			{
				public static string PushTrigger(EncounterTriggers trigger)
				{
					return "push_" + TriggerNormalized(trigger);
				}
			}
		}

		public static class Delay
		{
			public enum Triggers
			{
				Unknown = 0,
				Time = 10
			}

			public static class EnumKeys
			{
				public const string Trigger = "trigger";
			}

			public static class FloatKeys
			{
				public const string TimeDuration = "time_duration";
			}
		}
	}
}