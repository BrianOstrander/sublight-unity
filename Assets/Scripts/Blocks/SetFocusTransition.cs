using UnityEngine;

namespace LunraGames.SubLight
{
	public struct SetFocusTransition
	{
		public readonly SetFocusLayers Layer;
		public readonly SetFocusBlock Start;
		public readonly SetFocusBlock End;

		public readonly int Order;

		public SetFocusTransition(
			SetFocusLayers layer,
			SetFocusBlock start,
			SetFocusBlock end
		)
		{
			Layer = layer;
			Start = start;
			End = end;

			if (start.Order == end.Order) Order = start.Order;
			else Order = Mathf.Max(start.Order, end.Order);
		}

		public bool NoTransition { get { return Layer == SetFocusLayers.Unknown; } }
	}
}