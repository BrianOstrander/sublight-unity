using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class InventoryIdFilterEntryModel : ValueFilterEntryModel<string>
	{
		[JsonProperty] string inventoryId;

		[JsonIgnore]
		public ListenerProperty<string> InventoryId;

		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.InventoryId; } }

		public InventoryIdFilterEntryModel()
		{
			InventoryId = new ListenerProperty<string>(value => inventoryId = value, () => inventoryId);
		}
	}
}