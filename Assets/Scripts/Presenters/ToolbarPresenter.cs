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

			View.Selection = 0;
			View.Buttons = new ToolbarButtonBlock[] 
			{
				new ToolbarButtonBlock(LanguageStringModel.Override("Navigation"), View.GetIcon(SetFocusLayers.System), null),
				new ToolbarButtonBlock(LanguageStringModel.Override("Logistics"), View.GetIcon(SetFocusLayers.Room), null),
				new ToolbarButtonBlock(LanguageStringModel.Override("Communications"), View.GetIcon(SetFocusLayers.Communications), null),
				new ToolbarButtonBlock(LanguageStringModel.Override("Encyclopedia"), View.GetIcon(SetFocusLayers.Toolbar), null)
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