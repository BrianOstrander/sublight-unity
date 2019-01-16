using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterEventHandlerModel : EncounterHandlerModel<EncounterEventEncounterLogModel>
	{
		[JsonProperty] EncounterEventEntryModel[] events;

		[JsonIgnore]
		public readonly ListenerProperty<EncounterEventEntryModel[]> Events;

		public EncounterEventHandlerModel()
		{
			Events = new ListenerProperty<EncounterEventEntryModel[]>(value => events = value, () => events);
		}
	}
}