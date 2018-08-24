using Newtonsoft.Json;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class EncounterFocusRequest : FocusRequest
	{
		public static EncounterFocusRequest Encounter(States state = States.Request)
		{
			return new EncounterFocusRequest(state);
		}

		public override Focuses Focus { get { return Focuses.Encounter; } }

		[JsonConstructor]
		EncounterFocusRequest(States state = States.Request) : base(state) {}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new EncounterFocusRequest(state == States.Unknown ? State : state);
		}
	}
}