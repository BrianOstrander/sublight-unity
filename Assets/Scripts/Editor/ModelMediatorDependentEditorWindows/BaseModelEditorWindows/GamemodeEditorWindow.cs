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

		protected override void AssignModelName(GamemodeInfoModel model, string name)
		{
			// TODO: Do something here?
//			model.Name.Value = name;
		}
		#endregion
	}
}