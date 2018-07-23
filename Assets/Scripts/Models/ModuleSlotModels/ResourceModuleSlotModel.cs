using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ResourceModuleSlotModel : ModuleSlotModel
	{
		[JsonProperty] ResourceInventoryModel refillResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel refillLogisticsResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel maximumLogisticsResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel maximumResources = ResourceInventoryModel.Zero;

		/// <summary>
		/// The amount of resources this module should create or consume when 
		/// active, always.
		/// </summary>
		/// <value>Resources created or consumed per day.</value>
		[JsonIgnore]
		public ResourceInventoryModel RefillResources { get { return refillResources; } }
		/// <summary>
		/// The amount of resources this module should created when active and
		/// logistics space available. Should not be negative.
		/// </summary>
		/// <value>Resources created per day.</value>
		[JsonIgnore]
		public ResourceInventoryModel RefillLogisticsResources { get { return refillLogisticsResources; } }
		/// <summary>
		/// Gets amount this module increases the logistics maximum by.
		/// </summary>
		/// <value>The maximum logistics resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel MaximumLogisticsResources { get { return maximumLogisticsResources; } }
		/// <summary>
		/// The amount this module increases the maximum storable resources by.
		/// </summary>
		/// <value>The maximum resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel MaximumResources { get { return maximumResources; } }

		public override SlotTypes SlotType { get { return SlotTypes.Resource; } }
		public override bool IsFillable { get { return false; } }
	}
}