using System;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct XButtonToggleBlock
	{
		public static XButtonToggleBlock Default 
		{ 
			get 
			{
				return new XButtonToggleBlock
				{
					ActiveOnDisabled = true,
					ActiveOnNormal = true,
					ActiveOnHighlighted = true,
					ActiveOnPressed = true
				};
			} 
		}

		public bool ActiveOnDisabled;
		public bool ActiveOnNormal;
		public bool ActiveOnHighlighted;
		public bool ActiveOnPressed;

		public XButtonToggleBlock Duplicate
		{
			get
			{
				return new XButtonToggleBlock
				{
					ActiveOnDisabled = ActiveOnDisabled,
					ActiveOnNormal = ActiveOnNormal,
					ActiveOnHighlighted = ActiveOnHighlighted,
					ActiveOnPressed = ActiveOnPressed
				};
			}
		}
	}
}