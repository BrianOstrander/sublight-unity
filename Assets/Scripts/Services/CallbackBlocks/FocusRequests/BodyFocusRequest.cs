using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
	public class BodyFocusRequest : FocusRequest
	{
		public override Focuses Focus { get { return Focuses.Body; } }

		[JsonProperty] public readonly UniversePosition System;
		[JsonProperty] public readonly int Body;

		public BodyFocusRequest(
			UniversePosition system,
			int body,
			States state = States.Request
		) : base(state) 
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