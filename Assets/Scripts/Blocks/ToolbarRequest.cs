using System;

namespace LunraGames.SubLight
{
	public struct ToolbarSelectionRequest
	{
		public static ToolbarSelectionRequest Create(
			ToolbarSelections selection,
			bool locked,
			Sources source,
			Action done = null
		)
		{
			return new ToolbarSelectionRequest(
				false,
				selection,
				locked,
				source,
				done
			);
		}

		public static ToolbarSelectionRequest CreateInstant(
			ToolbarSelections selection,
			bool locked,
			Sources source,
			Action done = null
		)
		{
			return new ToolbarSelectionRequest(
				true,
				selection,
				locked,
				source,
				done
			);
		}

		public enum Sources
		{
			Unknown = 0,
			Player = 10,
			Encounter = 20
		}

		public readonly bool Instant;
		public readonly ToolbarSelections Selection;
		public readonly bool Locked;
		public readonly Sources Source;
		public readonly Action Done;

		ToolbarSelectionRequest(
			bool instant,
			ToolbarSelections selection,
			bool locked,
			Sources source,
			Action done
		)
		{
			Instant = instant;
			Selection = selection;
			Locked = locked;
			Source = source;
			Done = done ?? ActionExtensions.Empty;
		}
	}
}