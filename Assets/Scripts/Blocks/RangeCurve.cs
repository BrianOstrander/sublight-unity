using System;

using UnityEngine;
using UnityEngine.Serialization;

using LunraGames;

namespace LunraGames.SubLight
{
	public struct RangeResult
	{
		public float Min;
		public float Max;
		public float Value;

		public RangeResult(float min, float max, float value)
		{
			Min = min;
			Max = max;
			Value = value;
		}
	}

	[Serializable]
	public class RangeCurve
	{
		public AnimationCurve Min = AnimationCurveExtensions.Constant();
		public AnimationCurve Max = AnimationCurveExtensions.Constant(1f);

		public RangeResult Evaluate(float time, float lerp = 0f)
		{
			lerp = Mathf.Clamp01(lerp);
			var primary = Min.Evaluate(time);
			var secondary = Max.Evaluate(time);
			var min = Mathf.Min(primary, secondary);
			var max = Mathf.Max(primary, secondary);
			return new RangeResult
			{
				Min = min,
				Max = max,
				Value = min + ((max - min) * lerp)
			};
		}
	}
}