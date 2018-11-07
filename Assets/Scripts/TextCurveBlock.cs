using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct TextCurveBlock
	{
		public string Text;
		public GalaxyLabelTypes LabelType;
		public float FontSize;
		public Vector3 BeginAnchor;
		public Vector3 EndAnchor;
		public AnimationCurve Curve;
		public float CurveMaximum;
		public bool FlipAnchors;
		public bool FlipCurve;
	}
}