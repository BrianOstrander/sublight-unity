using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEdgeModel : EdgeModel
	{
		[JsonProperty] bool showFiltering;

		[JsonProperty] ButtonEntryModel entry = new ButtonEntryModel();
		[JsonIgnore]
		public ButtonEntryModel Entry { get { return entry; } }

		[JsonIgnore]
		public readonly ListenerProperty<bool> ShowFiltering;

		public ButtonEdgeModel()
		{
			ShowFiltering = new ListenerProperty<bool>(value => showFiltering = value, () => showFiltering);
		}

		public override EdgeEntryModel RawEntry { get { return Entry; } }
		public override string EdgeName { get { return "Button"; } }
	}
}