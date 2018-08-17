using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class InventoryOperationModel : Model
	{
		[JsonProperty] string operationId;

		/// <summary>
		/// The operation identifier, used internally for keeping track of this
		/// entry. This is NOT related to the inventory item or instance itself.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> OperationId;

		[JsonIgnore]
		public abstract InventoryOperations Operation { get; }

		public InventoryOperationModel()
		{
			OperationId = new ListenerProperty<string>(value => operationId = value, () => operationId);
		}
	}
}