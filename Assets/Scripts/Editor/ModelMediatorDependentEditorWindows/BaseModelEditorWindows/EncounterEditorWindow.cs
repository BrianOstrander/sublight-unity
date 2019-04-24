using System;

using UnityEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class EncounterEditorWindow : BaseModelEditorWindow<EncounterEditorWindow, EncounterInfoModel>
	{
		[MenuItem("Window/SubLight/Encounter Editor")]
		static void Initialize() { OnInitialize("Encounter Editor"); }

		public EncounterEditorWindow() : base("LG_SL_EncounterEditor_", "Encounter")
		{
			GeneralConstruct();
			LogsConstruct();
		}

		#region Model Overrides
		protected override EncounterInfoModel CreateModel(string name)
		{
			var model = base.CreateModel(name);

			model.RandomWeightMultiplier.Value = 1f;
			model.RandomAppearance.Value = 1f;
			model.Trigger.Value = EncounterTriggers.TransitComplete;

			return model;
		}

		protected override void AssignModelName(EncounterInfoModel model, string name)
		{
			model.Name.Value = name;
		}
		#endregion

		protected override bool CanEditDuringPlaymode()
		{
			return true;
		}
	}
}