using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct ColorStyleBlock
	{
		public static ColorStyleBlock Default
		{
			get
			{
				return new ColorStyleBlock
				{
					globalStyle = null,
					color = Color.white
				};
			}
		}

		[SerializeField]
		ColorStyleObject globalStyle;
		[SerializeField]
		Color color;

		public ColorStyleObject GlobalStyle { get { return globalStyle; } set { globalStyle = value; } }
		public Color Color { get { return globalStyle == null ? color : globalStyle.Color; } }

		public static implicit operator Color(ColorStyleBlock b)
		{
			return b.Color;
		}
	}
}