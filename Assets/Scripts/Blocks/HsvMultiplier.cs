using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct HsvMultiplier
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		public bool EnabledH;
		public float H;
		public bool EnabledS;
		public float S;
		public bool EnabledV;
		public float V;

		public Color Apply(Color value)
		{
			float? newH = null;
			float? newS = null;
			float? newV = null;

			if (EnabledH) newH = value.GetH() * H;
			if (EnabledS) newS = value.GetS() * S;
			if (EnabledV) newV = value.GetV() * V;

			return value.NewHsva(newH, newS, newV);
		}
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
	}
}