using System;

using UnityEngine;

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

		/// <summary>
		/// The state of the current request.
		/// </summary>
		/// <remarks>
		/// No finaly "Active" is sent, only a "Complete", so the final camera
		/// update is processed on that "Complete".
		/// </remarks>
		public States State;
		public UniversePosition Position;
		public UniversePosition Origin;
		public UniversePosition Destination;
		public DateTime StartTime;
		public DateTime EndTime;
		public TimeSpan Duration;
		/// <summary>
		/// Progress of the camera request, from 0.0 to 1.0.
		/// </summary>
		/// <remarks>
		/// While not used in the actual camera logic, this is kept as a useful
		/// hook for other listeners.
		/// </remarks>
		public float Progress;
		public bool Instant;

		public static SystemCameraRequest Request(
			UniversePosition position,
			UniversePosition origin,
			UniversePosition destination,
			DateTime startTime,
			DateTime endTime
		)
		{
			return new SystemCameraRequest(
				States.Request,
				position,
				origin,
				destination,
				startTime,
				endTime,
				0f,
				false);
		}

		public static SystemCameraRequest RequestInstant(UniversePosition position)
		{
			return new SystemCameraRequest(
				States.Request,
				position,
				position,
				position,
				DateTime.MinValue,
				DateTime.MinValue,
				0f,
				true);
		}

		public SystemCameraRequest(
			States state,
			UniversePosition position,
			UniversePosition origin,
			UniversePosition destination,
			DateTime startTime,
			DateTime endTime,
			float progress,
			bool instant)
		{
			State = state;
			Position = position;
			Origin = origin;
			Destination = destination;
			StartTime = startTime;
			EndTime = endTime;
			Progress = progress;
			Instant = instant;

			Duration = endTime - startTime;
		}

		public float GetProgress(DateTime current)
		{
			var total = (float)Duration.TotalSeconds;
			var elapsed = (float)(current - StartTime).TotalSeconds;
			return Mathf.Min(1f, elapsed / total);
		}

		public SystemCameraRequest Duplicate(States state = States.Unknown)
		{
			return new SystemCameraRequest(
				state == States.Unknown ? State : state,
				Position,
				Origin,
				Destination,
				StartTime,
				EndTime,
				Progress,
				Instant
			);
		}
	}
}