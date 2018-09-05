using System.Collections.Generic;

namespace LunraGames.SubLight
{
	public partial class GameState
	{
		static class Focuses
		{
			static SetFocusBlock GetRoomVisible(int order = 0)
			{
				var details = new RoomFocusDetails();
				return new SetFocusBlock(
					details,
					true,
					order,
					1f
				);
			}

			static SetFocusBlock GetToolbarVisible(int order = 0)
			{
				var details = new ToolbarFocusDetails();
				return new SetFocusBlock(
					details,
					true,
					order,
					1f
				);
			}

			static SetFocusBlock GetSystemVisible(int order = 0)
			{
				var details = new SystemFocusDetails();
				return new SetFocusBlock(
					details,
					true,
					order,
					1f
				);
			}

			static List<SetFocusBlock> DefaultVisibles
			{
				get
				{
					var results = new List<SetFocusBlock>();

					results.Add(GetRoomVisible());
					results.Add(GetToolbarVisible());

					return results;
				}
			}

			public static SetFocusBlock[] Defaults
			{
				get
				{
					var results = new List<SetFocusBlock>();

					results.Add(SetFocusBlock.Default<RoomFocusDetails>());
					results.Add(SetFocusBlock.Default<ToolbarFocusDetails>());
					results.Add(SetFocusBlock.Default<SystemFocusDetails>());
					results.Add(SetFocusBlock.Default<ShipFocusDetails>());

					return results.ToArray();
				}
			}

			public static SetFocusBlock[] System
			{
				get
				{
					var results = DefaultVisibles;

					results.Add(GetSystemVisible(1));

					return results.ToArray();
				}
			}
		}
	}
}