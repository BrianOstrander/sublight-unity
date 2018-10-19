using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct ToolbarButtonBlock
	{
		public LanguageStringModel Text;
		public Sprite Icon;
		public Action SelectClick;
		public bool Interactable;

		public ToolbarButtonBlock(LanguageStringModel text, Sprite icon, Action selectClick, bool interactable = true)
		{
			Text = text;
			Icon = icon;
			SelectClick = selectClick;
			Interactable = interactable;
		}
	}
}