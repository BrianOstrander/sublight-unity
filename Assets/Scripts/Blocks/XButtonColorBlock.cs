using UnityEngine;
using System;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct XButtonColorBlock
	{
		public static XButtonColorBlock Default 
		{ 
			get 
			{
				return new XButtonColorBlock
				{
					DisabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f),
					NormalColor = Color.white,
					HighlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f),
					PressedColor = new Color(0.78f, 0.78f, 0.78f)
				};
			} 
		}

		public Color DisabledColor;
		public Color NormalColor;
		public Color HighlightedColor;
		public Color PressedColor;
	}
}