using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEdgeModel : EdgeModel
	{
		[JsonProperty] ButtonEntryModel entry = new ButtonEntryModel();

		[JsonIgnore]
		public ButtonEntryModel Entry { get { return entry; } }

		public override EdgeEntryModel RawEntry { get { return Entry; } }
		public override string EdgeName { get { return "Button"; } }
	}
}