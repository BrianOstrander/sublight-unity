using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class InteractedEncounterInfoModel : Model
	{
		[JsonProperty] string encounterId;
		[JsonProperty] int timesSeen;
		[JsonProperty] int timesCompleted;
		[JsonProperty] DateTime lastSeen = DateTime.MinValue;

		[JsonIgnore]
		public readonly ListenerProperty<string> EncounterId;
		[JsonIgnore]
		public readonly ListenerProperty<int> TimesSeen;
		[JsonIgnore]
		public readonly ListenerProperty<int> TimesCompleted;
		[JsonIgnore]
		public readonly ListenerProperty<DateTime> LastSeen;

		public InteractedEncounterInfoModel()
		{
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			TimesSeen = new ListenerProperty<int>(value => timesSeen = value, () => timesSeen);
			TimesCompleted = new ListenerProperty<int>(value => timesCompleted = value, () => timesCompleted);
			LastSeen = new ListenerProperty<DateTime>(value => lastSeen = value, () => lastSeen);
		}
	}
}