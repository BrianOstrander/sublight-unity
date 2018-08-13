using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class InventoryReferenceEditorWindow
	{
		void OnEditReference(OrbitalCrewReferenceModel reference)
		{
			OnRefrenceHeader<OrbitalCrewReferenceModel, OrbitalCrewInventoryModel>(reference, "Orbital Crew");

			var model = reference.Model.Value;

			// We do this so to reset any beforeSave logic... should probably move somewhere else though...
			beforeSave = null;

			OnEditCrewShared(model);


		}
	}
}