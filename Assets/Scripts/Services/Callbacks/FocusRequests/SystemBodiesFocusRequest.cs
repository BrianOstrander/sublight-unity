using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
	public class SystemBodiesFocusRequest : FocusRequest
	{
		public override Focuses Focus { get { return Focuses.SystemBodies; } }

		[JsonProperty] public readonly UniversePosition System;

		public SystemBodiesFocusRequest(
			UniversePosition system,
			States state = States.Request
		) : base(state) 
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