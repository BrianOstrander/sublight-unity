using UnityEngine;

using Newtonsoft.Json;

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

		/// <summary>
		/// The state of the current request.
		/// </summary>
		/// <remarks>
		/// No finaly "Active" is sent, only a "Complete", so the final travel
		/// update is processed on that "Complete".
		/// </remarks>
		[JsonProperty] public readonly States State;
		[JsonProperty] public readonly UniversePosition Position;
		[JsonProperty] public readonly UniversePosition Origin;
		[JsonProperty] public readonly UniversePosition Destination;
		[JsonProperty] public readonly DayTime StartTime;
		[JsonProperty] public readonly DayTime EndTime;
		[JsonProperty] public readonly DayTime Duration;
		[JsonProperty] public readonly float FuelConsumed;
		/// <summary>
		/// Progress of the travel request, from 0.0 to 1.0.
		/// </summary>
		/// <remarks>
		/// While not used in the actual travel logic, this is kept as a useful
		/// hook for other listeners.
		/// </remarks>
		[JsonProperty] public readonly float Progress;

		public TravelRequest(
			States state,
			UniversePosition position,
			UniversePosition origin,
			UniversePosition destination,
			DayTime startTime,
			DayTime endTime,
			float fuelConsumed,
			float progress)
		{
			State = state;
			Position = position;
			Origin = origin;
			Destination = destination;
			StartTime = startTime;
			EndTime = endTime;
			FuelConsumed = fuelConsumed;
			Progress = progress;

			Duration = DayTime.Elapsed(startTime, endTime);
		}

		public float GetProgress(DayTime current)
		{
			var total = Duration.TotalTime;
			var elapsed = DayTime.Elapsed(StartTime, current).TotalTime;
			return Mathf.Min(1f, elapsed / total);
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
				FuelConsumed,
				Progress
			);
		}
	}
}