using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class AddResourceOperationModel : InventoryOperationModel
	{
		[JsonProperty] ResourceInventoryModel value = ResourceInventoryModel.Zero;

		[JsonIgnore]
		public ResourceInventoryModel Value { get { return value; } }

		public override InventoryOperations Operation { get { return InventoryOperations.AddResource; } }
	}
}