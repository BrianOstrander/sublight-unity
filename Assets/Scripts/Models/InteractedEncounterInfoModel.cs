using System;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class InteractedEncounterInfoModel : Model
	{
		[JsonProperty] string encounterId;
		[JsonProperty] int totalTimesSeen;
		[JsonProperty] int uniqueEndingsSeen;
		[JsonProperty] DateTime lastSeen = DateTime.MinValue;

		[JsonIgnore]
		public readonly ListenerProperty<string> EncounterId;
		[JsonIgnore]
		public readonly ListenerProperty<int> TotalTimesSeen;
		[JsonIgnore]
		public readonly ListenerProperty<int> UniqueEndingsSeen;
		[JsonIgnore]
		public readonly ListenerProperty<DateTime> LastSeen;

		public InteractedEncounterInfoModel()
		{
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			TotalTimesSeen = new ListenerProperty<int>(value => totalTimesSeen = value, () => totalTimesSeen);
			UniqueEndingsSeen = new ListenerProperty<int>(value => uniqueEndingsSeen = value, () => uniqueEndingsSeen);
			LastSeen = new ListenerProperty<DateTime>(value => lastSeen = value, () => lastSeen);
		}
	}
}