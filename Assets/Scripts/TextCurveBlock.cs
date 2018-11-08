using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct TextCurveBlock
	{
		public static TextCurveBlock Default
		{
			get
			{
				return new TextCurveBlock
				{
					Text = String.Empty,
					LabelStyle = GalaxyLabelStyles.Bold,
					FontSize = 16f,
					Curve = AnimationCurveExtensions.Constant(),
					CurveMaximum = 1f
				};
			}
		}

		public string Text;
		public GalaxyLabelStyles LabelStyle;
		public float FontSize;
		public AnimationCurve Curve;
		public float CurveMaximum;
		public bool FlipAnchors;
		public bool FlipCurve;
	}
}