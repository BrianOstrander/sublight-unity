using System;

using UnityEngine;
using UnityEngine.UI;

namespace LunraGames.SpaceFarm
{
	[Serializable]
	public class XButtonStyleBlock
	{
		public static XButtonStyleBlock Default { get { return new XButtonStyleBlock(); } }

		public AnimationCurve DistanceScaleIntensity = AnimationCurve.Linear(0f, 0f, 1f, 0f);
		public AnimationCurve ClickScaleDelta = AnimationCurve.Linear(0f, 0f, 1f, 0f);
		public XButtonColorBlock Colors = XButtonColorBlock.Default;
		public XButtonToggleBlock Toggles = XButtonToggleBlock.Default;
		public AnimationCurve ClickDistanceDelta = AnimationCurve.Linear(0f, 0f, 1f, 0f);
	}
}