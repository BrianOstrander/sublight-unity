using System;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Serialization;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct FloatRange
	{
		public static FloatRange Zero { get { return new FloatRange(0f, 0f); } }

		[FormerlySerializedAs("x"), SerializeField]
		float primary;
		[FormerlySerializedAs("y"), SerializeField]
		float secondary;

		[JsonIgnore]
		public float Primary { get { return primary; } }
		[JsonIgnore]
		public float Secondary { get { return secondary; } }
		[JsonIgnore]
		public float Delta { get { return Secondary - Primary; } }

		public FloatRange(float primary, float secondary)
		{
			this.primary = primary;
			this.secondary = secondary;
		}

		public FloatRange NewPrimary(float primary) { return new FloatRange(primary, Secondary); }
		public FloatRange NewSecondary(float secondary) { return new FloatRange(Primary, secondary); }
		public float Evaluate(float value) { return Primary + (Delta * value); }
		public float EvaluateClamped(float value) { return Evaluate(Mathf.Clamp01(value)); }
		public float Progress(float value)
		{
			if (Mathf.Approximately(0f, Delta)) return 1f;
			return (value - primary) / Delta;
		}
		public float ProgressClamped(float value) { return Progress(Mathf.Clamp(value, Mathf.Min(Primary, Secondary), Mathf.Max(Primary, Secondary))); }
	}
}