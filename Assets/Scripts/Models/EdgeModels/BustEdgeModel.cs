using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class BustEdgeModel : EdgeModel
	{
		[JsonProperty] BustEntryModel entry = new BustEntryModel();

		[JsonIgnore]
		public BustEntryModel Entry { get { return entry; } }

		public override EdgeEntryModel RawEntry { get { return Entry; } }
		public override string EdgeName
		{
			get
			{
				return (string.IsNullOrEmpty(entry.BustId.Value) ? "< Missing Id >" : entry.BustId.Value) + "." + entry.BustEvent.Value;
			}
		}
	}
}