using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class BodyFocusRequest : FocusRequest
	{
		public readonly SystemModel System;
		public readonly BodyModel Body;

		public BodyFocusRequest(
			SystemModel system,
			BodyModel body,
			States state = States.Request
		) : base(Focuses.Body, state) 
		{
			System = system;
			Body = body;
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new BodyFocusRequest(
				System,
				Body,
				state == States.Unknown ? State : state
			);
		}
	}
}