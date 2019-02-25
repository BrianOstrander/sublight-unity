using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterEventEntryModel : EdgeEntryModel
	{
		[JsonProperty] EncounterEvents.Types encounterEvent;
		[JsonProperty] bool isHalting;

		[JsonIgnore]
		public readonly ListenerProperty<EncounterEvents.Types> EncounterEvent;
		[JsonIgnore]
		public readonly ListenerProperty<bool> IsHalting;

		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore]
		public KeyValueListModel KeyValues { get { return keyValues; } }

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default(true);
		[JsonIgnore]
		public ValueFilterModel Filtering { get { return filtering; } }

		public EncounterEventEntryModel()
		{
			EncounterEvent = new ListenerProperty<EncounterEvents.Types>(value => encounterEvent = value, () => encounterEvent);
			IsHalting = new ListenerProperty<bool>(value => isHalting = value, () => isHalting);
		}
	}
}