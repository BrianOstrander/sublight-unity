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
		public readonly float TimeDelta;

		public Gesture(Vector2 begin, bool isSecondary, float timeDelta)
		{
			State = States.Begin;
			IsSecondary = isSecondary;
			Begin = begin;
			Current = begin;
			End = begin;
			Delta = Vector2.zero;
			TimeDelta = timeDelta;
		}

		public Gesture(Vector2 begin, Vector2 end, States state, bool isSecondary, float timeDelta)
		{
			State = state;
			IsSecondary = isSecondary;
			Begin = begin;
			Current = end;
			End = end;
			Delta = end - begin;
			TimeDelta = timeDelta;
		}
	}
}