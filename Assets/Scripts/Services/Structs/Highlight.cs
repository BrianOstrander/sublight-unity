using UnityEngine;

namespace LunraGames.SpaceFarm
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

		public States State { get; private set; }
		public GameObject[] GameObjects { get; private set; }
		public string Tooltip { get; private set; }

		public Highlight(States state, GameObject[] gameObjects = null, string tooltip = null)
		{
			State = state;
			GameObjects = gameObjects ?? new GameObject[0];
			Tooltip = tooltip;
		}
	}
}