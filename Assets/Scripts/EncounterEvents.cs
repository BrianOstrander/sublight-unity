using System;

namespace LunraGames.SubLight
{
	public static class EncounterEvents
	{
		public enum Types
		{
			Unknown = 0,
			Custom = 10,
			ToolbarSelection = 20
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