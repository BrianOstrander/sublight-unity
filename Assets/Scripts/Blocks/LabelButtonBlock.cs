using System;

namespace LunraGames.SpaceFarm
{
	public struct LabelButtonBlock
	{
		public string Text;
		public Action Click;
		public bool Interactable;

		public LabelButtonBlock(string text, Action click, bool interactable = true)
		{
			Text = text;
			Click = click;
			Interactable = interactable;
		}
	}
}