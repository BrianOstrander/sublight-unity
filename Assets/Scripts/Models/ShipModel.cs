using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ShipModel : Model
	{
		#region Serialized
		[JsonProperty] InventoryListModel inventory = new InventoryListModel();
		[JsonProperty] UniversePosition position;

		[JsonIgnore]
		public InventoryListModel Inventory { get { return inventory; } }
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;
		#endregion

		public ShipModel()
		{
			// Assigned Values
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
		}

		#region Events

		#endregion
	}
}