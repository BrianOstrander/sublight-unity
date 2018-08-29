using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class AddRandomInstanceOperationModel : InventoryOperationModel
	{
		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();

		[JsonIgnore]
		public readonly ListenerProperty<ValueFilterModel> Filtering;

		public override InventoryOperations Operation { get { return InventoryOperations.AddRandomInstance; } }

		public AddRandomInstanceOperationModel()
		{
			Filtering = new ListenerProperty<ValueFilterModel>(value => filtering = value, () => filtering);
		}
	}
}