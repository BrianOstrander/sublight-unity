using System;

namespace LunraGames.SubLight
{
	public struct SetFocusBlock
	{
		public readonly bool Active;
		public readonly SetFocusLayers Layer;
		public readonly int Order;
		public readonly float Weight;
		public readonly SetFocusDetailsBase Details;
	}
}