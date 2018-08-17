using UnityEngine;

namespace LunraGames.SubLight
{
	public struct Gesture
	{
		public readonly bool IsGesturing;
		public readonly bool IsSecondary;
		public readonly Vector2 Begin;
		public readonly Vector2 Current;
		public readonly Vector2 End;
		public readonly Vector2 Delta;

		public Gesture(Vector2 begin, bool isSecondary)
		{
			IsGesturing = true;
			IsSecondary = isSecondary;
			Begin = begin;
			Current = begin;
			End = begin;
			Delta = Vector2.zero;
		}

		public Gesture(Vector2 begin, Vector2 end, bool isGesturing, bool isSecondary)
		{
			IsGesturing = isGesturing;
			IsSecondary = isSecondary;
			Begin = begin;
			Current = end;
			End = end;
			Delta = end - begin;
		}
	}
}