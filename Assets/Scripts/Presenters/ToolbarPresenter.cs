using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ToolbarPresenter : Presenter<IToolbarView>, IPresenterCloseShowOptions
	{
		public void Show(Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			View.Buttons = new ToolbarButtonBlock[] 
			{
				new ToolbarButtonBlock(LanguageStringModel.Override("lol"), View.GetIcon(SetFocusLayers.System), false, null),
				new ToolbarButtonBlock(LanguageStringModel.Override("lol"), View.GetIcon(SetFocusLayers.Room), false, null),
				new ToolbarButtonBlock(LanguageStringModel.Override("lol"), View.GetIcon(SetFocusLayers.Communications), false, null),
				new ToolbarButtonBlock(LanguageStringModel.Override("lol"), View.GetIcon(SetFocusLayers.Toolbar), false, null)
			};

			ShowView(parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);
		}
		#region Events
		#endregion
	}
}