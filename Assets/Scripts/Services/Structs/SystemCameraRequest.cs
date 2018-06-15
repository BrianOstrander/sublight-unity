namespace LunraGames.SpaceFarm
{
	public struct SystemCameraRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		public States State;
		public UniversePosition FocusPosition;
		public UniversePosition FocusOrigin;
		public UniversePosition FocusDestination;
		public DayTime StartTime;
		public DayTime EndTime;
		public DayTime Duration;
		public float Progress;
		public bool Instant;

		public static SystemCameraRequest Request(
			UniversePosition focusPosition,
			UniversePosition focusOrigin,
			UniversePosition focusDestination,
			DayTime startTime,
			DayTime endTime
		)
		{
			return new SystemCameraRequest(
				States.Request,
				focusPosition,
				focusOrigin,
				focusDestination,
				startTime,
				endTime,
				0f,
				false);
		}

		public static SystemCameraRequest RequestInstant(UniversePosition focusPosition)
		{
			return new SystemCameraRequest(
				States.Request,
				focusPosition,
				focusPosition,
				focusPosition,
				DayTime.Zero,
				DayTime.Zero,
				0f,
				true);
		}

		SystemCameraRequest(
			States state,
			UniversePosition focusPosition,
			UniversePosition focusOrigin,
			UniversePosition focusDestination,
			DayTime startTime,
			DayTime endTime,
			float progress,
			bool instant)
		{
			State = state;
			FocusPosition = focusPosition;
			FocusOrigin = focusOrigin;
			FocusDestination = focusDestination;
			StartTime = startTime;
			EndTime = endTime;
			Progress = progress;
			Instant = instant;

			Duration = DayTime.DayTimeElapsed(startTime, endTime);
		}

		public SystemCameraRequest Duplicate(States state = States.Unknown)
		{
			return new SystemCameraRequest(
				state == States.Unknown ? State : state,
				FocusPosition,
				FocusOrigin,
				FocusDestination,
				StartTime,
				EndTime,
				Progress,
				Instant
			);
		}
	}
}