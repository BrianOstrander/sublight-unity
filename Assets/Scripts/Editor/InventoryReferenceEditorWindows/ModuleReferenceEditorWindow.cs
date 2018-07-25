using UnityEngine;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public partial class InventoryReferenceEditorWindow
	{
		void OnEditModule(ModuleReferenceModel reference)
		{
			OnRefrenceHeader<ModuleReferenceModel, ModuleInventoryModel>(reference, "Module");
		}
	}
}