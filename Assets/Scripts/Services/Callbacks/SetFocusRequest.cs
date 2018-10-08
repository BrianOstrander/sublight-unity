using System;

namespace LunraGames.SubLight
{
	public struct SetFocusRequest
	{
		public static SetFocusRequest Request(float duration, SetFocusBlock[] targets, Action done = null)
		{
			return new SetFocusRequest(false, false, duration, targets, done);
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