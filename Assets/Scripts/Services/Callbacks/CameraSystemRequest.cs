using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct CameraSystemRequest
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
		public readonly States State;
		public readonly UniversePosition Position;
		public readonly UniversePosition Origin;
		public readonly UniversePosition Destination;
		public readonly DateTime StartTime;
		public readonly DateTime EndTime;
		public readonly TimeSpan Duration;
		/// <summary>
		/// Progress of the camera request, from 0.0 to 1.0.
		/// </summary>
		/// <remarks>
		/// While not used in the actual camera logic, this is kept as a useful
		/// hook for other listeners.
		/// </remarks>
		public readonly float Progress;
		public readonly bool Instant;

		public static CameraSystemRequest Request(
			UniversePosition position,
			UniversePosition origin,
			UniversePosition destination,
			DateTime startTime,
			DateTime endTime
		)
		{
			return new CameraSystemRequest(
				States.Request,
				position,
				origin,
				destination,
				startTime,
				endTime,
				0f,
				false);
		}

		public static CameraSystemRequest RequestInstant(UniversePosition position)
		{
			return new CameraSystemRequest(
				States.Request,
				position,
				position,
				position,
				DateTime.MinValue,
				DateTime.MinValue,
				0f,
				true);
		}

		public CameraSystemRequest(
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

		public CameraSystemRequest Duplicate(States state = States.Unknown)
		{
			return new CameraSystemRequest(
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