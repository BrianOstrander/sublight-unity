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
			DeliverFocusBlock[] gatherResults,
			SetFocusBlock[] transitions,
			DateTime startTime,
			DateTime endTime
		)
		{
			return new TransitionFocusRequest(
				States.Request,
				gatherResults,
				transitions,
				startTime,
				endTime,
				endTime - startTime,
				0f,
				false
			);
		}

		public static TransitionFocusRequest RequestInstant(
			DeliverFocusBlock[] gatherResults,
			SetFocusBlock[] transitions
		)
		{
			return new TransitionFocusRequest(
				States.Request,
				gatherResults,
				transitions,
				DateTime.MinValue,
				DateTime.MinValue,
				TimeSpan.MinValue,
				0f,
				true
			);
		}

		public readonly States State;
		public readonly DeliverFocusBlock[] GatherResults;
		public readonly SetFocusBlock[] Transitions;
		public readonly DateTime StartTime;
		public readonly DateTime EndTime;
		public readonly TimeSpan Duration;
		public readonly float Progress;
		public readonly bool Instant;

		public TransitionFocusRequest(
			States state,
			DeliverFocusBlock[] gatherResults,
			SetFocusBlock[] transitions,
			DateTime startTime,
			DateTime endTime,
			TimeSpan duration,
			float progress,
			bool instant
		)
		{
			State = state;
			GatherResults = gatherResults;
			Transitions = transitions;
			StartTime = startTime;
			EndTime = endTime;
			Duration = duration;
			Progress = progress;
			Instant = instant;
		}

		public TransitionFocusRequest Duplicate(States state = States.Unknown)
		{
			return new TransitionFocusRequest(
				state == States.Unknown ? State : state,
				GatherResults,
				Transitions,
				StartTime,
				EndTime,
				Duration,
				Progress,
				Instant
			);
		}
	}
}