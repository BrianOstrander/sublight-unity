namespace LunraGames.SpaceFarm.Models
{
	public class ModuleReferenceModel : InventoryReferenceModel<ModuleInventoryModel>
	{
		public ModuleReferenceModel() : base(SaveTypes.ModuleReference) {}
	}
}