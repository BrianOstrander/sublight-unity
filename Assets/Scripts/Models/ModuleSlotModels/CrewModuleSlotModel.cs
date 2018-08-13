using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class CrewModuleSlotModel : ModuleSlotModel
	{
		[JsonProperty] InventoryTypes[] validInventoryTypes = new InventoryTypes[0];

		[JsonIgnore]
		public readonly ListenerProperty<InventoryTypes[]> ValidInventoryTypes;

		public override SlotTypes SlotType { get { return SlotTypes.Crew; } }

		public override bool CanSlot(InventoryTypes inventoryType) { return ValidInventoryTypes.Value.Contains(inventoryType); }

		public CrewModuleSlotModel()
		{
			ValidInventoryTypes = new ListenerProperty<InventoryTypes[]>(value => validInventoryTypes = value, () => validInventoryTypes);
		}
	}
}