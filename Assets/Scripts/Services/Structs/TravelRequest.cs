using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public struct TravelRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		public States State;
		public UniversePosition Position;
		public SystemModel Origin;
		public SystemModel Destination;
		public DayTime StartTime;
		public DayTime EndTime;
		public DayTime Duration;
		public float Progress;

		public TravelRequest(
			States state,
			UniversePosition position,
			SystemModel origin,
			SystemModel destination,
			DayTime startTime,
			DayTime endTime,
			float progress)
		{
			State = state;
			Position = position;
			Origin = origin;
			Destination = destination;
			StartTime = startTime;
			EndTime = endTime;
			Progress = progress;

			Duration = DayTime.DayTimeElapsed(startTime, endTime);
		}

		public TravelRequest Duplicate(States state = States.Unknown)
		{
			return new TravelRequest(
				state == States.Unknown ? State : state,
				Position,
				Origin,
				Destination,
				StartTime,
				EndTime,
				Progress
			);
		}
	}
}