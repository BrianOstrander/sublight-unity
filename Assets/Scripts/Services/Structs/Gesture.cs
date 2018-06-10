using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct Gesture
	{
		public bool IsGesturing { get; private set; }
		public bool IsSecondary { get; private set;  }
		public Vector2 Begin { get; private set; }
		public Vector2 Current { get; private set; }
		public Vector2 End { get; private set; }
		public Vector2 Delta { get; private set; }

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