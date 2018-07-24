namespace LunraGames.SpaceFarm.Models
{
	public class OrbitalCrewReferenceModel : InventoryReferenceModel<OrbitalCrewInventoryModel>
	{
		public OrbitalCrewReferenceModel() : base(SaveTypes.OrbitalCrewReference) {}
	}
}