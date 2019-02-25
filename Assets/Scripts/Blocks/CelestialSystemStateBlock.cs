using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct CelestialSystemStateBlock
	{
		public static CelestialSystemStateBlock Default
		{
			get
			{
				return new CelestialSystemStateBlock(States.Idle, UniversePosition.Zero, null);
			}
		}

		public static CelestialSystemStateBlock Idle(UniversePosition position, SystemModel system)
		{
			return new CelestialSystemStateBlock(States.Idle, position, system);
		}

		public static CelestialSystemStateBlock Highlight(UniversePosition position, SystemModel system)
		{
			return new CelestialSystemStateBlock(States.Highlighted, position, system);
		}

		public static CelestialSystemStateBlock Select(UniversePosition position, SystemModel system)
		{
			return new CelestialSystemStateBlock(States.Selected, position, system);
		}

		public static CelestialSystemStateBlock UnSelect(UniversePosition position, SystemModel system)
		{
			return new CelestialSystemStateBlock(States.UnSelected, position, system);
		}

		public enum States
		{
			Unknown = 0,
			Idle = 10,
			Highlighted = 20,
			Selected = 30,
			UnSelected = 40
		}

		public States State;
		public UniversePosition Position;
		public SystemModel System;

		CelestialSystemStateBlock(
			States state,
			UniversePosition position,
			SystemModel system
		)
		{
			State = state;
			Position = position;
			System = system;
		}
	}
}
