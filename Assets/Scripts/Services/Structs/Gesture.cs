using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct Gesture
	{
		public bool IsGesturing { get; private set; }
		public Vector2 Begin { get; private set; }
		public Vector2 Current { get; private set; }
		public Vector2 End { get; private set; }
		public Vector2 Delta { get; private set; }

		public Gesture(Vector2 begin)
		{
			IsGesturing = true;
			Begin = begin;
			Current = begin;
			End = begin;
			Delta = Vector2.zero;
		}

		public Gesture(Vector2 begin, Vector2 end, bool isGesturing)
		{
			IsGesturing = isGesturing;
			Begin = begin;
			Current = end;
			End = end;
			Delta = end - begin;
		}
	}
}