using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class SystemBodiesFocusRequest : FocusRequest
	{
		public SystemModel System;

		public SystemBodiesFocusRequest(
			SystemModel system,
			States state = States.Request
		) : base(Focuses.SystemBodies, state) 
		{
			System = system;
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new SystemBodiesFocusRequest(
				System,
				state == States.Unknown ? State : state
			);
		}
	}
}