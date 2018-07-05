using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class InventoryModel : Model
	{
		[JsonProperty] string inventoryId;
		[JsonProperty] string name;

		/// <summary>
		/// Gets the type of the inventory item.
		/// </summary>
		/// <value>The type of the inventory item.</value>
		[JsonIgnore]
		public abstract InventoryTypes InventoryType { get; }

		[JsonIgnore]
		public readonly ListenerProperty<string> InventoryId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;

		public InventoryModel()
		{
			InventoryId = new ListenerProperty<string>(value => inventoryId = value, () => inventoryId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
		}
	}
}