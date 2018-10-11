using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class DestructionOriginSystemPresenter : Presenter<ISystemDestructionOriginView>, IPresenterCloseShow
	{
		public void Show()
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = UniversePosition.Zero;
			View.Highlight = OnHighlight;
			View.Click = OnClick;
			ShowView(instant: true);
		}

		public void Close()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView();
		}

		#region Events
		void OnHighlight(bool highlighted)
		{
			//App.Log("Highlight", LogTypes.ToDo);
		}

		void OnClick()
		{
			App.Callbacks.DialogRequest(DialogRequest.Alert(LanguageStringModel.Override(Strings.HomeInfo), title: LanguageStringModel.Override(Strings.HomeInfoTitle)));
		}
		#endregion
	}
}