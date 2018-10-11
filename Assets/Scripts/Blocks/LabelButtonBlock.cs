using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct LabelButtonBlock
	{
		public LanguageStringModel Text;
		public Action Click;
		public bool Interactable;

		public LabelButtonBlock(LanguageStringModel text, Action click, bool interactable = true)
		{
			Text = text;
			Click = click;
			Interactable = interactable;
		}
	}
}