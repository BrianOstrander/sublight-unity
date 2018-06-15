using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public struct CameraRequest
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
		public SystemModel FocusOrigin;
		public SystemModel FocusDestination;
		public DayTime StartTime;
		public DayTime EndTime;
		public DayTime Duration;
		public float Progress;

		public CameraRequest(
			States state,
			UniversePosition focusPosition,
			SystemModel focusOrigin,
			SystemModel focusDestination,
			DayTime startTime,
			DayTime endTime,
			float progress)
		{
			State = state;
			FocusPosition = focusPosition;
			FocusOrigin = focusOrigin;
			FocusDestination = focusDestination;
			StartTime = startTime;
			EndTime = endTime;
			Progress = progress;

			Duration = DayTime.DayTimeElapsed(startTime, endTime);
		}

		public CameraRequest Duplicate(States state = States.Unknown)
		{
			return new CameraRequest(
				state == States.Unknown ? State : state,
				FocusPosition,
				FocusOrigin,
				FocusDestination,
				StartTime,
				EndTime,
				Progress
			);
		}
	}
}