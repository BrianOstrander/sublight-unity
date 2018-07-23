using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class CrewModuleSlotModel : ModuleSlotModel
	{
		[JsonProperty] InventoryTypes[] validInventoryTypes = new InventoryTypes[0];

		[JsonIgnore]
		public readonly ListenerProperty<InventoryTypes[]> ValidInventoryTypes;

		public override SlotTypes SlotType { get { return SlotTypes.Crew; } }

		public CrewModuleSlotModel()
		{
			ValidInventoryTypes = new ListenerProperty<InventoryTypes[]>(value => validInventoryTypes = value, () => validInventoryTypes);
		}
	}
}