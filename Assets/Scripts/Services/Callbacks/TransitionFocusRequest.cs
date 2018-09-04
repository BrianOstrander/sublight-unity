using System;

namespace LunraGames.SubLight
{
	public struct TransitionFocusRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		public static TransitionFocusRequest Request(
			GatherFocusResult gatherResult,
			SetFocusTransition[] transitions,
			DateTime startTime,
			DateTime endTime
		)
		{
			return new TransitionFocusRequest(
				States.Request,
				gatherResult,
				transitions,
				startTime,
				endTime,
				endTime - startTime,
				0f,
				false
			);
		}

		public static TransitionFocusRequest RequestInstant(
			GatherFocusResult gatherResult,
			SetFocusTransition[] transitions
		)
		{
			return new TransitionFocusRequest(
				States.Request,
				gatherResult,
				transitions,
				DateTime.MinValue,
				DateTime.MinValue,
				TimeSpan.MinValue,
				0f,
				true
			);
		}

		public readonly States State;
		public readonly GatherFocusResult GatherResult;
		public readonly SetFocusTransition[] Transitions;
		public readonly DateTime StartTime;
		public readonly DateTime EndTime;
		public readonly TimeSpan Duration;
		public readonly float Progress;
		public readonly bool Instant;

		public TransitionFocusRequest(
			States state,
			GatherFocusResult gatherResult,
			SetFocusTransition[] transitions,
			DateTime startTime,
			DateTime endTime,
			TimeSpan duration,
			float progress,
			bool instant
		)
		{
			State = state;
			GatherResult = gatherResult;
			Transitions = transitions;
			StartTime = startTime;
			EndTime = endTime;
			Duration = duration;
			Progress = progress;
			Instant = instant;
		}

		public TransitionFocusRequest Duplicate(
			States state = States.Unknown,
			float? progress = null
		)
		{
			return new TransitionFocusRequest(
				state == States.Unknown ? State : state,
				GatherResult,
				Transitions,
				StartTime,
				EndTime,
				Duration,
				progress.HasValue ? progress.Value : Progress,
				Instant
			);
		}
	}
}