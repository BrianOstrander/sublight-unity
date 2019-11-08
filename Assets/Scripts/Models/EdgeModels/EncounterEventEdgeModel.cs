using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterEventEdgeModel : EdgeModel
	{
		[JsonProperty] EncounterEvents.Types encounterEvent;
		[JsonProperty] bool isHalting;

		[JsonIgnore]
		public readonly ListenerProperty<EncounterEvents.Types> EncounterEvent;
		[JsonIgnore]
		public readonly ListenerProperty<bool> IsHalting;

		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore]
		public KeyValueListModel KeyValues => keyValues;

		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default(true);
		[JsonIgnore]
		public ValueFilterModel Filtering => filtering;

		public override string EdgeName => EncounterEvent.Value.ToString();
		
		public EncounterEventEdgeModel()
		{
			EncounterEvent = new ListenerProperty<EncounterEvents.Types>(value => encounterEvent = value, () => encounterEvent);
			IsHalting = new ListenerProperty<bool>(value => isHalting = value, () => isHalting);
		}
	}
}