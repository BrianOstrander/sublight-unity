using System;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct CurveRange
	{
		public static CurveRange Normal { get { return new CurveRange(FloatRange.Normal, AnimationCurveExtensions.LinearNormal()); } }
		public static CurveRange InverseNormal { get { return new CurveRange(FloatRange.Normal, AnimationCurveExtensions.LinearInverseNormal()); } }

		[SerializeField]
		FloatRange range;
		[SerializeField]
		AnimationCurve curve;

		[JsonIgnore]
		public FloatRange Range { get { return range; } }
		[JsonIgnore]
		public AnimationCurve Curve { get { return curve; } }

		public CurveRange(FloatRange range, AnimationCurve curve)
		{
			this.range = range;
			this.curve = curve;
		}

		/// <summary>
		/// Takes a normal value, evaluates it along the curve, then finds where
		/// that value lies between the range's Primary and Secondary values.
		/// </summary>
		/// <remarks>
		/// Use the EvaluateClamped method to ensure the result is between the
		/// Primary and Secondary values.
		/// </remarks>
		/// <returns>The evaluate.</returns>
		/// <param name="normal">Normal.</param>
		public float Evaluate(float normal) { return Range.Evaluate(Curve.Evaluate(normal)); }

		/// <summary>
		/// Takes a value, clampse it between 0.0 and 1.0, and evaluates it
		/// along a curve before finding that value between the Primary and
		/// Secondary values.
		/// </summary>
		/// <returns>The clamped.</returns>
		/// <param name="normal">Normal.</param>
		public float EvaluateClamped(float normal) { return Range.Evaluate(Curve.Evaluate(Mathf.Clamp01(normal))); }
	}
}