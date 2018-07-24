namespace LunraGames.SpaceFarm.Models
{
	public class ModuleModuleSlotModel : ModuleSlotModel
	{
		public override SlotTypes SlotType { get { return SlotTypes.Module; } }

		public override bool CanSlot(InventoryTypes inventoryType) { return inventoryType == InventoryTypes.Module; }
	}
}