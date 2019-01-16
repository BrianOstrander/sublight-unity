using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterEventEntryModel : Model
	{
		[JsonProperty] string eventId;
		[JsonProperty] EncounterEvents.Types encounterEvent;

		[JsonIgnore]
		public readonly ListenerProperty<EncounterEvents.Types> EncounterEvent;
		[JsonIgnore]
		public readonly ListenerProperty<string> EventId;

		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore]
		public KeyValueListModel KeyValues { get { return keyValues; } }

		public EncounterEventEntryModel()
		{
			EncounterEvent = new ListenerProperty<EncounterEvents.Types>(value => encounterEvent = value, () => encounterEvent);
			EventId = new ListenerProperty<string>(value => eventId = value, () => eventId);
		}
	}
}