using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct ToolbarButtonBlock
	{
		public LanguageStringModel Text;
		public Sprite Icon;
		public Action Click;
		public bool Interactable;

		public ToolbarButtonBlock(LanguageStringModel text, Sprite icon, Action click, bool interactable = true)
		{
			Text = text;
			Icon = icon;
			Click = click;
			Interactable = interactable;
		}
	}
}