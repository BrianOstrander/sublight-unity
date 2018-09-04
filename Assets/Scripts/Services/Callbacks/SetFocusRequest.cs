using System;

namespace LunraGames.SubLight
{
	public struct SetFocusRequest
	{
		public static SetFocusRequest Request(float duration, SetFocusBlock[] targets, Action done = null)
		{
			return new SetFocusRequest(duration, targets, done);
		}

		public readonly float Duration;
		public readonly SetFocusBlock[] Targets;
		public readonly Action Done;

		SetFocusRequest(float duration, SetFocusBlock[] targets, Action done)
		{
			Duration = duration;
			Targets = targets;
			Done = done ?? ActionExtensions.Empty;
		}
	}
}