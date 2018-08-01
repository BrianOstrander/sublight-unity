using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class AddInstanceOperationModel : InventoryOperationModel
	{
		[JsonProperty] string inventoryId;

		[JsonIgnore]
		public readonly ListenerProperty<string> InventoryId;

		public override InventoryOperations Operation { get { return InventoryOperations.AddInstance; } }

		public AddInstanceOperationModel()
		{
			InventoryId = new ListenerProperty<string>(value => inventoryId = value, () => inventoryId);
		}
	}
}