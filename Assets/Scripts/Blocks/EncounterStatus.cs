using System;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm
{
	[Serializable]
	public struct EncounterStatus
	{
		public enum States
		{
			Unknown = 0,
			NeverSeen = 10,
			Seen = 20,
			Completed = 30
		}

		public static EncounterStatus NeverSeen(string encounterId) { return new EncounterStatus(encounterId, States.NeverSeen); }
		public static EncounterStatus Seen(string encounterId) { return new EncounterStatus(encounterId, States.Seen); }
		public static EncounterStatus Completed(string encounterId) { return new EncounterStatus(encounterId, States.Completed); }

		[JsonProperty] public readonly string Encounter;
		[JsonProperty] public readonly States State;

		public EncounterStatus(string encounter, States state)
		{
			Encounter = encounter;
			State = state;
		}
	}
}