using UnityEngine;

namespace LunraGames.SubLight
{
	public struct ScrollGesture
	{
		public readonly bool IsGesturing;
		public readonly bool IsSecondary;
		public readonly Vector2 Begin;
		public readonly Vector2 Current;
		public readonly Vector2 End;
		public readonly Vector2 Delta;

		public readonly float TimeDelta;

		public ScrollGesture(Vector2 begin, bool isSecondary, float timeDelta)
		{
			IsGesturing = true;
			IsSecondary = isSecondary;
			Begin = begin;
			Current = begin;
			End = begin;
			Delta = Vector2.zero;

			TimeDelta = timeDelta;
		}

		public ScrollGesture(Vector2 begin, Vector2 end, bool isGesturing, bool isSecondary, float timeDelta)
		{
			IsGesturing = isGesturing;
			IsSecondary = isSecondary;
			Begin = begin;
			Current = end;
			End = end;
			Delta = end - begin;

			TimeDelta = timeDelta;
		}
	}
}