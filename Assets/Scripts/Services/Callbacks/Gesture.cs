using UnityEngine;

namespace LunraGames.SubLight
{
	public struct Gesture
	{
		public enum States
		{
			Unknown = 0,
			Begin = 10,
			Active = 20,
			End = 30
		}

		public readonly States State;
		public readonly bool IsSecondary;
		public readonly Vector2 Begin;
		public readonly Vector2 Current;
		public readonly Vector2 End;
		public readonly Vector2 Delta;
		public readonly Vector2 DeltaSinceLast;

		public readonly float TimeDelta;

		/// <summary>
		/// This makes it so gestures are normalized by the framerate, basically
		/// turning the Current into scrolls per second.
		/// </summary>
		public readonly Vector2 CurrentScaledByDelta;
		/// <summary>
		/// Scaled by framerate.
		/// </summary>
		public readonly Vector2 DeltaSinceLastScaledByDelta;

		public Gesture(
			Vector2 begin,
			bool isSecondary,
			float timeDelta
		)
		{
			State = States.Begin;
			IsSecondary = isSecondary;
			Begin = begin;
			Current = begin;
			End = begin;
			Delta = Vector2.zero;
			DeltaSinceLast = Vector2.zero;
			TimeDelta = timeDelta;

			var timeDeltaScalar = 1f / timeDelta;

			CurrentScaledByDelta = begin * timeDeltaScalar;
			DeltaSinceLastScaledByDelta = DeltaSinceLast * timeDeltaScalar;
		}

		public Gesture(
			Vector2 begin,
			Vector2 end,
			Vector2 deltaSinceLast,
			States state,
			bool isSecondary,
			float timeDelta
		)
		{
			State = state;
			IsSecondary = isSecondary;
			Begin = begin;
			Current = end;
			End = end;
			Delta = end - begin;
			DeltaSinceLast = deltaSinceLast;
			TimeDelta = timeDelta;

			var timeDeltaScalar = 1f / timeDelta;

			CurrentScaledByDelta = end * timeDeltaScalar;
			DeltaSinceLastScaledByDelta = DeltaSinceLast * timeDeltaScalar;
		}

		/// <summary>
		/// Gestures go from -1 to 1, but viewport goes from 0 to 1. This converts to viewport.
		/// </summary>
		/// <returns>The viewport.</returns>
		/// <param name="gestureNormal">Normal.</param>
		public static Vector2 GetViewport(Vector2 gestureNormal) { return (gestureNormal + new Vector2(1f, 1f)) * 0.5f; }
	}
}