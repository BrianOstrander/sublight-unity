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
		protected override void AssignModelName(GamemodeInfoModel model, string name)
		{
			model.Name.Value = name;
		}
		#endregion
	}
}