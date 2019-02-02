using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueEdgeModel : EdgeModel
	{
		[JsonProperty] KeyValueEntryModel entry = new KeyValueEntryModel();

		[JsonIgnore]
		public KeyValueEntryModel Entry { get { return entry; } }

		public override EdgeEntryModel RawEntry { get { return Entry; } }
		public override string EdgeName { get { return Entry.Operation.Value.ToString(); } }
	}
}