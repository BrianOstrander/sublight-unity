﻿using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public struct SaveRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Complete = 20
		}

		public static SaveRequest Save() { return new SaveRequest(States.Request); }

		public States State;

		public SaveRequest(States state)
		{
			State = state;
		}

		public SaveRequest Duplicate(States state = States.Unknown)
		{
			return new SaveRequest(
				state == States.Unknown ? State : state
			);
		}

	}
}