using UnityEngine;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public partial class InventoryReferenceEditorWindow
	{
		void OnEditReference(OrbitalCrewReferenceModel reference)
		{
			OnRefrenceHeader<OrbitalCrewReferenceModel, OrbitalCrewInventoryModel>(reference, "Orbital Crew");
		}
	}
}