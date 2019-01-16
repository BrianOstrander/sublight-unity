using System;

namespace LunraGames.SubLight
{
	public struct ToolbarSelectionRequest
	{
		public static ToolbarSelectionRequest Create(
			ToolbarSelections selection,
			Action done = null
		)
		{
			return new ToolbarSelectionRequest(
				false,
				selection,
				done
			);
		}

		public static ToolbarSelectionRequest CreateInstant(
			ToolbarSelections selection,
			Action done = null
		)
		{
			return new ToolbarSelectionRequest(
				true,
				selection,
				done
			);
		}

		public readonly bool Instant;
		public readonly ToolbarSelections Selection;
		public readonly Action Done;

		ToolbarSelectionRequest(
			bool instant,
			ToolbarSelections selection,
			Action done
		)
		{
			Instant = instant;
			Selection = selection;
			Done = done ?? ActionExtensions.Empty;
		}
	}
}