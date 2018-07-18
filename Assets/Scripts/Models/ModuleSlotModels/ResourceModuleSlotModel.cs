using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ResourceModuleSlotModel : ModuleSlotModel
	{
		[JsonProperty] ResourceInventoryModel maximumResources = ResourceInventoryModel.Zero;

		[JsonIgnore]
		public readonly ListenerProperty<ResourceInventoryModel> MaximumResources;

		public override SlotTypes SlotType { get { return SlotTypes.Resource; } }
		public override bool IsFillable { get { return false; } }

		public ResourceModuleSlotModel()
		{
			MaximumResources = new ListenerProperty<ResourceInventoryModel>(value => maximumResources = value, () => maximumResources);
		}
	}
}