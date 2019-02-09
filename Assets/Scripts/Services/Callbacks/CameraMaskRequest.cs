using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct CameraMaskRequest
	{
		public const float DefaultRevealDuration = 0.75f;
		public const float DefaultHideDuration = 0.2f;

		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		public static CameraMaskRequest Reveal(float duration = 0f, Action done = null)
		{
			return new CameraMaskRequest(States.Request, duration, 0f, true, done);
		}

		public static CameraMaskRequest Hide(float duration = 0f, Action done = null)
		{
			return new CameraMaskRequest(States.Request, duration, 0f, false, done);
		}

		public readonly States State;
		public readonly float Duration;
		public readonly float Elapsed;
		public readonly float Progress;
		public readonly bool Revealing;
		public readonly Action Done;

		public bool IsInstant { get { return Mathf.Approximately(0f, Duration); } }

		CameraMaskRequest(
			States state,
			float duration,
			float elapsed,
			bool revealing,
			Action done
		)
		{
			State = state;
			Duration = duration;
			Elapsed = elapsed;
			Revealing = revealing;
			Done = done ?? ActionExtensions.Empty;

			Progress = Mathf.Approximately(0f, Duration) ? 1f : elapsed / duration;
		}

		public CameraMaskRequest Duplicate(
			States state = States.Unknown,
			float? elapsed = null
		)
		{
			return new CameraMaskRequest(
				state == States.Unknown ? State : state,
				Duration,
				elapsed.HasValue ? elapsed.Value : Elapsed,
				Revealing,
				Done
			);
		}
	}
}