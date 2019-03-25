using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct SetFocusRequest
	{
		const float DefaultDuration = 0.16f;

		public static SetFocusRequest Request(SetFocusBlock[] targets, Action done = null, float duration = DefaultDuration)
		{
			var instant = Mathf.Approximately(0f, duration);
			if (instant) Debug.LogWarning("Requesting a set focus with a duration of zero, running instantly");

			return new SetFocusRequest(false, instant, duration, targets, done);
		}

		public static SetFocusRequest RequestInstant(SetFocusBlock[] targets, Action done = null)
		{
			return new SetFocusRequest(false, true, 0f, targets, done);
		}

		public static SetFocusRequest Default(SetFocusBlock[] targets, Action done = null)
		{
			return new SetFocusRequest(true, true, 0f, targets, done);
		}

		public readonly bool IsDefault;
		public readonly bool Instant;
		public readonly float Duration;
		public readonly SetFocusBlock[] Targets;
		public readonly Action Done;

		SetFocusRequest(
			bool isDefault,
			bool instant,
			float duration,
			SetFocusBlock[] targets,
			Action done
		)
		{
			IsDefault = isDefault;
			Instant = instant;
			Duration = duration;
			Targets = targets;
			Done = done ?? ActionExtensions.Empty;
		}
	}
}