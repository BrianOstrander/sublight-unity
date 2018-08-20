using Newtonsoft.Json;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class EncounterFocusRequest : FocusRequest
	{
		public static EncounterFocusRequest Encounter(
			string encounter,
			UniversePosition system,
			KeyValueListModel keyValues = null,
			States state = States.Request
		)
		{
			return new EncounterFocusRequest(encounter, system, keyValues ?? new KeyValueListModel(), state);
		}

		[JsonProperty] public readonly string EncounterId;
		[JsonProperty] public readonly UniversePosition System;
		[JsonProperty] public readonly KeyValueListModel KeyValues;

		public override Focuses Focus { get { return Focuses.Encounter; } }

		[JsonConstructor]
		EncounterFocusRequest(
			string encounterId,
			UniversePosition system,
			KeyValueListModel keyValues = null,
			States state = States.Request
		) : base(state)
		{
			EncounterId = encounterId;
			System = system;
			KeyValues = keyValues ?? new KeyValueListModel();
		}

		public override FocusRequest Duplicate(States state = States.Unknown)
		{
			return new EncounterFocusRequest(
				EncounterId,
				System,
				KeyValues,
				state == States.Unknown ? State : state
			);
		}
	}
}