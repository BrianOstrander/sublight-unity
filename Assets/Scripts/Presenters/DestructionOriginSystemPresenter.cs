using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class DestructionOriginSystemPresenter : Presenter<ISystemDestructionOriginView>
	{
		GameModel model;

		public DestructionOriginSystemPresenter(GameModel model)
		{
			this.model = model;

		}

		protected override void UnBind()
		{
			base.UnBind();

		}

		public void Show()
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = UniversePosition.Zero;
			View.Highlight = OnHighlight;
			View.Click = OnClick;
			ShowView(instant: true);
		}

		#region Events
		void OnHighlight(bool highlighted)
		{
			//App.Log("Highlight", LogTypes.ToDo);
		}

		void OnClick()
		{
			App.Callbacks.DialogRequest(DialogRequest.Alert(Strings.HomeInfo, Strings.HomeInfoTitle));
		}
		#endregion
	}
}