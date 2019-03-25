using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class TransitView : View, ITransitView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float prepareDuration;
		[SerializeField]
		float transitWarmupDuration;
		[SerializeField]
		float transitCooldownDuration;
		[SerializeField]
		Vector2 transitDuration;
		[SerializeField]
		float finalizeDuration;

		[SerializeField]
		AnimationCurve transitTimeCurve;
		[SerializeField]
		int transitTimeWarmupDays;
		[SerializeField]
		int transitTimeCooldownDays;

		[Header("Animation Static")]
		[SerializeField]
		AnimationCurve transitDistanceCurve;
		[SerializeField]
		AnimationCurve transitVelocityCurve;
		[SerializeField]
		AnimationCurve transitTimeScalarCurve;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public float PrepareDuration { get { return prepareDuration; } }
		public float GetTransitDuration(float scalar = 0f) { return transitCooldownDuration + transitWarmupDuration + (transitDuration.x + ((transitDuration.y - transitDuration.x) * scalar)); }
		public float FinalizeDuration { get { return finalizeDuration; } }

		public AnimationCurve TransitDistanceCurve { get { return transitDistanceCurve; } }
		public AnimationCurve TransitVelocityCurve { get { return transitVelocityCurve; } }
		public AnimationCurve TransitTimeScalarCurve { get { return transitTimeScalarCurve; } }

		public void GetTimeProgress(
			float transitProgress,
			float scalar,
			RelativeDayTime duration,
			out float timeProgress,
			out RelativeDayTime elapsed
		)
		{
			var totalDuration = GetTransitDuration(scalar);

			var warmupMaximum = transitWarmupDuration / totalDuration;
			var middleMaximum = 1f - (transitCooldownDuration / totalDuration);

			var warmup = new RelativeDayTime(
				new DayTime(Mathf.Min(transitTimeWarmupDays, duration.ShipTime.TotalTime * 0.3f)),
				new DayTime(Mathf.Min(transitTimeWarmupDays, duration.GalacticTime.TotalTime * 0.3f))
			);

			var cooldown = new RelativeDayTime(
				new DayTime(Mathf.Min(transitTimeCooldownDays, duration.ShipTime.TotalTime * 0.3f)),
				new DayTime(Mathf.Min(transitTimeCooldownDays, duration.GalacticTime.TotalTime * 0.3f))
			);

			var middle = duration - (warmup + cooldown);

			if (transitProgress < warmupMaximum)
			{
				elapsed = warmup * transitTimeCurve.Evaluate(transitProgress / warmupMaximum);
			}
			else if (transitProgress < middleMaximum)
			{
				var middleProgress = 1f + ((transitProgress - warmupMaximum) / (middleMaximum - warmupMaximum));
				elapsed = warmup + (middle * (transitTimeCurve.Evaluate(middleProgress) - 1f));
			}
			else
			{
				var cooldownProgress = 2f + ((transitProgress - middleMaximum) / (1f - middleMaximum));
				elapsed = warmup + middle + (cooldown * (transitTimeCurve.Evaluate(cooldownProgress) - 2f));
			}
			timeProgress = elapsed.ShipTime.TotalTime / duration.ShipTime.TotalTime;
		}
	}

	public interface ITransitView : IView
	{
		float PrepareDuration { get; }
		float GetTransitDuration(float scalar = 0f);
		float FinalizeDuration { get; }

		AnimationCurve TransitDistanceCurve { get; }
		AnimationCurve TransitVelocityCurve { get; }
		AnimationCurve TransitTimeScalarCurve { get; }

		void GetTimeProgress(
			float transitProgress,
			float scalar,
			RelativeDayTime duration,
			out float timeProgress,
			out RelativeDayTime elapsed
		);
	}
}