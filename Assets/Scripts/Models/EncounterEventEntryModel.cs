using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterEventEntryModel : Model
	{
		[JsonProperty] string eventId;
		[JsonProperty] EncounterEvents encounterEvent;

		[JsonIgnore]
		public readonly ListenerProperty<EncounterEvents> EncounterEvent;
		[JsonIgnore]
		public readonly ListenerProperty<string> EventId;

		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore]
		public KeyValueListModel KeyValues { get { return keyValues; } }

		public EncounterEventEntryModel()
		{
			EncounterEvent = new ListenerProperty<EncounterEvents>(value => encounterEvent = value, () => encounterEvent);
			EventId = new ListenerProperty<string>(value => eventId = value, () => eventId);
		}
	}
}