using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncyclopediaEdgeModel : EdgeModel
	{
		[JsonProperty] EncyclopediaEntryModel entry = new EncyclopediaEntryModel();

		[JsonIgnore]
		public EncyclopediaEntryModel Entry { get { return entry; } }

		public override EdgeEntryModel RawEntry { get { return Entry; } }
		public override string EdgeName { get { return string.IsNullOrEmpty(entry.Header.Value) ? "Introduction" : "Section"; } }
	}
}