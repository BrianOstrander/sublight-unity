using System;

using UnityEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class ModuleTraitEditorWindow : BaseModelEditorWindow<ModuleTraitEditorWindow, ModuleTraitModel>
	{
		[MenuItem("Window/SubLight/Module Trait Editor")]
		static void Initialize() { OnInitialize("Module Trait Editor"); }

		public ModuleTraitEditorWindow() : base("LG_SL_ModuleTraitEditor_", "Module Trait")
		{
			//GeneralConstruct();
		}

		#region Model Overrides
		protected override ModuleTraitModel CreateModel(string name)
		{
			var model = base.CreateModel(name);

			// Any overrides are here...

			return model;
		}

		protected override void AssignModelName(ModuleTraitModel model, string name)
		{
			model.Name.Value = name;
		}
		#endregion
	}
}