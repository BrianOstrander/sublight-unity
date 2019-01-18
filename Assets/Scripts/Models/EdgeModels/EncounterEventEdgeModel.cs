using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterEventEdgeModel : EdgeModel
	{
		[JsonProperty] EncounterEventEntryModel entry = new EncounterEventEntryModel();

		[JsonIgnore]
		public EncounterEventEntryModel Entry { get { return entry; } }

		public override EdgeEntryModel RawEntry { get { return Entry; } }
		public override string EdgeName { get { return Entry.EncounterEvent.Value.ToString(); } }
	}
}