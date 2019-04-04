using System;

using UnityEditor;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public partial class GamemodeEditorWindow : BaseModelEditorWindow<GamemodeEditorWindow, GamemodeInfoModel>
	{
		[MenuItem("Window/SubLight/Gamemode Editor")]
		static void Initialize() { OnInitialize("Gamemode Editor"); }

		public GamemodeEditorWindow() : base("LG_SL_GamemodeEditor_", "Gamemode")
		{
			GeneralConstruct();
		}

		#region Model Overrides
		protected override GamemodeInfoModel CreateModel(string name)
		{
			var model = base.CreateModel(name);

			// Any overrides are here...

			return model;
		}

		protected override void AssignModelId(GamemodeInfoModel model, string id)
		{
			model.GamemodeId.Value = model.SetMetaKey(MetaKeyConstants.GamemodeInfo.GamemodeId, Guid.NewGuid().ToString());
		}

		protected override void AssignModelName(GamemodeInfoModel model, string name)
		{
			model.Name.Value = name;
		}

		protected override string GetModelId(SaveModel model)
		{
			return model.GetMetaKey(MetaKeyConstants.GamemodeInfo.GamemodeId);
		}
		#endregion
	}
}