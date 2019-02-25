using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class DialogEdgeModel : EdgeModel
	{
		[JsonProperty] DialogEntryModel entry = new DialogEntryModel();

		[JsonIgnore]
		public DialogEntryModel Entry { get { return entry; } }

		public override EdgeEntryModel RawEntry { get { return Entry; } }
		public override string EdgeName { get { return "Dialog"; } }
	}
}