using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class InventoryTagFilterEntryModel : ValueFilterEntryModel<string>
	{
		[JsonProperty] string tag;

		[JsonIgnore]
		public ListenerProperty<string> Tag;

		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.InventoryTag; } }

		public InventoryTagFilterEntryModel()
		{
			Tag = new ListenerProperty<string>(value => tag = value, () => tag);
		}
	}
}