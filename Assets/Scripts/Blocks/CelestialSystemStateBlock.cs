using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunraGames.SubLight
{
	public struct CelestialSystemStateBlock
	{
		public static CelestialSystemStateBlock Default
		{
			get
			{
				return new CelestialSystemStateBlock(States.Idle, UniversePosition.Zero);
			}
		}

		public static CelestialSystemStateBlock Idle { get { return Default; } }

		public static CelestialSystemStateBlock Highlight(UniversePosition position)
		{
			return new CelestialSystemStateBlock(States.Highlighted, position);
		}

		public enum States
		{
			Unknown = 0,
			Idle = 10,
			Highlighted = 20
		}

		public States State;
		public UniversePosition Position;

		CelestialSystemStateBlock(States state, UniversePosition position)
		{
			State = state;
			Position = position;
		}
	}
}
