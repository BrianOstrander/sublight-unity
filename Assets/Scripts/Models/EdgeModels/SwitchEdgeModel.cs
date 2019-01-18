using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SwitchEdgeModel : EdgeModel
	{
		[JsonProperty] SwitchEntryModel entry = new SwitchEntryModel();

		[JsonIgnore]
		public SwitchEntryModel Entry { get { return entry; } }

		public override EdgeEntryModel RawEntry { get { return Entry; } }
		public override string EdgeName { get { return "Switch"; } }
	}
}