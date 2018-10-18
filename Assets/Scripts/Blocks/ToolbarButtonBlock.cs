using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct ToolbarButtonBlock
	{
		public LanguageStringModel Text;
		public Sprite Icon;
		public bool IsSelected;
		public Action SelectClick;
		public bool Interactable;

		public ToolbarButtonBlock(LanguageStringModel text, Sprite icon, bool isSelected, Action selectClick, bool interactable = true)
		{
			Text = text;
			Icon = icon;
			IsSelected = isSelected;
			SelectClick = selectClick;
			Interactable = interactable;
		}
	}
}