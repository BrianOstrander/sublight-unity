using Newtonsoft.Json;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class EncounterFocusRequest : FocusRequest
	{
		public static EncounterFocusRequest Encounter(
			string encounter,
			UniversePosition system,
			int body,
			string crew,
			KeyValueListModel keyValues = null,
			States state = States.Request
		)
		{
			return new EncounterFocusRequest(encounter, system, body, crew, keyValues ?? new KeyValueListModel(), state);
		}

		[JsonProperty] public readonly string EncounterId;
		[JsonProperty] public readonly UniversePosition System;
		[JsonProperty] public readonly int Body;
		[JsonProperty] public readonly string Crew;
		[JsonProperty] public readonly KeyValueListModel KeyValues;

		public override Focuses Focus { get { return Focuses.Encounter; } }

		[JsonConstructor]
		EncounterFocusRequest(
			string encounterId,
			UniversePosition system,
			int body,
			string crew,
			KeyValueListModel keyValues = null,
			States state = States.Request
		) : base(state)
		{
			EncounterId = encounterId;
			System = system;
			Body = body;
			Crew = crew;
			KeyValues = keyValues ?? new KeyValueListModel();
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new EncounterFocusRequest(
				EncounterId,
				System,
				Body,
				Crew,
				KeyValues,
				state == States.Unknown ? State : state
			);
		}
	}
}