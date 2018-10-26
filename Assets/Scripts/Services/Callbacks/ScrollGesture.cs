using UnityEngine;

namespace LunraGames.SubLight
{
	public struct ScrollGesture
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
		/// This makes it so scrolls are normalized by the framerate, basically
		/// turning the Current into scrolls per second.
		/// </summary>
		public readonly Vector2 CurrentScaledByDelta;

		public ScrollGesture(
			Vector2 end,
			bool isSecondary,
			float timeDelta
		)
		{
			State = States.Begin;
			IsSecondary = isSecondary;
			Begin = Vector2.zero;
			Current = end;
			End = end;
			Delta = end;
			DeltaSinceLast = end;
			TimeDelta = timeDelta;

			CurrentScaledByDelta = end * (1f / timeDelta);
		}

		public ScrollGesture(
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

			CurrentScaledByDelta = end * (1f / timeDelta);
		}
	}
}