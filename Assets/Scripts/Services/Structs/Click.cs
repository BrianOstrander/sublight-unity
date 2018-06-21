using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct Click
	{
		public readonly Vector2 Begin;
		public readonly Vector2 Current;
		public readonly Vector2 End;
		public readonly Vector2 Delta;
		public readonly bool ClickedNothing;

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