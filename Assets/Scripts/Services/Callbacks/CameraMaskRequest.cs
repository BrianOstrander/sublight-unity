using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct CameraMaskRequest
	{
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
		public readonly float Progress;
		public readonly float Scalar;
		public readonly bool Revealing;
		public readonly Action Done;

		public bool IsInstant { get { return Mathf.Approximately(0f, Duration); } }

		CameraMaskRequest(
			States state,
			float duration,
			float progress,
			bool revealing,
			Action done
		)
		{
			State = state;
			Duration = duration;
			Progress = progress;
			Revealing = revealing;
			Done = done ?? ActionExtensions.Empty;

			Scalar = Mathf.Approximately(0f, Duration) ? 1f : progress / duration;
		}

		public CameraMaskRequest Duplicate(
			States state = States.Unknown,
			float? progress = null
		)
		{
			return new CameraMaskRequest(
				state == States.Unknown ? State : state,
				Duration,
				progress.HasValue ? progress.Value : Progress,
				Revealing,
				Done
			);
		}
	}
}