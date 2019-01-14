using System;

using UnityEditor;
using UnityEngine;

using LunraGamesEditor;

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
			model.Trigger.Value = EncounterTriggers.Automatic;

			return model;
		}
		
		protected override void AssignModelId(EncounterInfoModel model, string id)
		{
			model.EncounterId.Value = model.SetMetaKey(MetaKeyConstants.EncounterInfo.EncounterId, Guid.NewGuid().ToString());
		}

		protected override void AssignModelName(EncounterInfoModel model, string name)
		{
			model.Name.Value = name;
		}

		protected override string GetModelId(SaveModel model)
		{
			return model.GetMetaKey(MetaKeyConstants.EncounterInfo.EncounterId);
		}
		#endregion
	}
}