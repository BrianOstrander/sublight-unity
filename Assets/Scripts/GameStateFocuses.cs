using System.Collections.Generic;

namespace LunraGames.SubLight
{
	public static class GameStateFocuses
	{
		public static SetFocusBlock[] Defaults
		{
			get
			{
				var results = new List<SetFocusBlock>();

				results.Add(SetFocusBlock.Default<ToolbarFocusDetails>());
				results.Add(SetFocusBlock.Default<SystemFocusDetails>());
				results.Add(SetFocusBlock.Default<ShipFocusDetails>());

				return results.ToArray();
			}
		}
	}
}
