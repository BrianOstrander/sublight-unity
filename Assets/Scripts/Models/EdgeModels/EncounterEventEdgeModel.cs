using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterEventEdgeModel : EdgeModel
	{
		[JsonProperty] EncounterEventEntryModel entry = new EncounterEventEntryModel();

		[JsonIgnore]
		public EncounterEventEntryModel Entry => entry;

		public override EdgeEntryModel RawEntry => Entry;
		public override string EdgeName => Entry.EncounterEvent.Value.ToString();
	}
}