using System;

using UnityEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class ProceduralNoiseEditorWindow : BaseModelEditorWindow<ProceduralNoiseEditorWindow, ProceduralNoiseModel>
	{
		[MenuItem("Window/SubLight/Procedural Noise Editor")]
		static void Initialize() { OnInitialize("Procedural Noise Editor"); }

		public ProceduralNoiseEditorWindow() : base("LG_SL_ProceduralNoiseEditor_", "Procedural Noise")
		{
			GeneralConstruct();
		}

		#region Model Overrides
		protected override ProceduralNoiseModel CreateModel(string name)
		{
			var model = base.CreateModel(name);

			// Set default values here...

			return model;
		}

		protected override void AssignModelId(ProceduralNoiseModel model, string id)
		{
			model.ProceduralNoiseId.Value = model.SetMetaKey(MetaKeyConstants.ProceduralNoise.ProceduralNoiseId, Guid.NewGuid().ToString());
		}

		protected override void AssignModelName(ProceduralNoiseModel model, string name)
		{
			model.Name.Value = name;
		}

		protected override string GetModelId(SaveModel model)
		{
			return model.GetMetaKey(MetaKeyConstants.ProceduralNoise.ProceduralNoiseId);
		}
		#endregion
	}
}