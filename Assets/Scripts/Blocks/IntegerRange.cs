using System;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Serialization;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct IntegerRange
	{
		public static IntegerRange Zero => new IntegerRange(0, 0);
		public static IntegerRange Normal => new IntegerRange(0, 1);
		public static IntegerRange Constant(int value) => new IntegerRange(value, value);

		[SerializeField]
		int primary;
		[SerializeField]
		int secondary;

		[JsonIgnore]
		public int Primary => primary;

		[JsonIgnore]
		public int Secondary => secondary;

		[JsonIgnore]
		public int Delta => Secondary - Primary;

		public IntegerRange(int primary, int secondary)
		{
			this.primary = primary;
			this.secondary = secondary;
		}

		public IntegerRange NewPrimary(int primary) { return new IntegerRange(primary, Secondary); }
		public IntegerRange NewSecondary(int secondary) { return new IntegerRange(Primary, secondary); }

		float EvaluateFloat(float normal) => Primary + (Delta * normal);

		public int EvaluateRound(float normal) => Mathf.RoundToInt(EvaluateFloat(normal));
		public int EvaluateFloor(float normal) => Mathf.FloorToInt(EvaluateFloat(normal));
		public int EvaluateCeiling(float normal) => Mathf.CeilToInt(EvaluateFloat(normal));
	}
}