using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct Click
	{
		public Vector2 Begin { get; private set; }
		public Vector2 Current { get; private set; }
		public Vector2 End { get; private set; }
		public Vector2 Delta { get; private set; }
		public bool ClickedNothing { get; private set; }

		public Click(Vector2 begin, Vector2 end, bool clickedNothing)
		{
			Begin = begin;
			Current = end;
			End = end;
			Delta = end - begin;
			ClickedNothing = clickedNothing;
		}
	}
}