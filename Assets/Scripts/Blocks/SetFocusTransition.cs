namespace LunraGames.SubLight
{
	public struct SetFocusTransition
	{
		public readonly SetFocusLayers Layer;
		public readonly SetFocusBlock Start;
		public readonly SetFocusBlock End;

		public SetFocusTransition(
			SetFocusLayers layer,
			SetFocusBlock start,
			SetFocusBlock end
		)
		{
			Layer = layer;
			Start = start;
			End = end;
		}
	}
}