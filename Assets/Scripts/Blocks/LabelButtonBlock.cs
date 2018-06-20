using System;

namespace LunraGames.SpaceFarm
{
	public struct LabelButtonBlock
	{
		public string Text;
		public Action Click;

		public LabelButtonBlock(string text, Action click)
		{
			Text = text;
			Click = click;
		}
	}
}