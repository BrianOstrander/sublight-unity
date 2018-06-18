using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SystemDestructionOriginPresenter : Presenter<ISystemDestructionOriginView>
	{
		GameModel gameModel;

		public SystemDestructionOriginPresenter(GameModel gameModel)
		{
			this.gameModel = gameModel;

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
			// TODO: Highlight logic
		}

		void OnClick()
		{
			// TODO: Click logic
		}
		#endregion
	}
}