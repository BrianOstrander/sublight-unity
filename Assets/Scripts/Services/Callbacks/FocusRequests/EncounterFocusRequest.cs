using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
	public class EncounterFocusRequest : FocusRequest
	{
		public static EncounterFocusRequest Encounter(
			string encounterId,
			UniversePosition system,
			int body,
			string crew,
			States state = States.Request
		)
		{
			return new EncounterFocusRequest(encounterId, system, body, crew, state);
		}

		[JsonProperty] public readonly string EncounterId;
		[JsonProperty] public readonly UniversePosition System;
		[JsonProperty] public readonly int Body;
		[JsonProperty] public readonly string Crew;

		public override Focuses Focus { get { return Focuses.Encounter; } }

		[JsonConstructor]
		EncounterFocusRequest(
			string encounterId,
			UniversePosition system,
			int body,
			string crew,
			States state = States.Request
		) : base(state)
		{
			EncounterId = encounterId;
			System = system;
			Body = body;
			Crew = crew;
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new EncounterFocusRequest(
				EncounterId,
				System,
				Body,
				Crew,
				state == States.Unknown ? State : state
			);
		}
	}
}