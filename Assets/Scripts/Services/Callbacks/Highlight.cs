using UnityEngine;

namespace LunraGames.SubLight
{
	public struct Highlight
	{
		public enum States
		{
			Unknown,
			Begin,
			Change,
			End
		}

		public readonly States State;
		public readonly GameObject[] GameObjects;
		public readonly string Tooltip;

		public Highlight(States state, GameObject[] gameObjects = null, string tooltip = null)
		{
			State = state;
			GameObjects = gameObjects ?? new GameObject[0];
			Tooltip = tooltip;
		}
	}
}