using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class MainMenuOptionsPresenter : Presenter<IMainMenuOptionsView>, IPresenterCloseShowOptions
	{
		LabelButtonBlock[] leftOptions;
		LabelButtonBlock[] rightOptions;

		public MainMenuOptionsPresenter(LabelButtonBlock[] leftOptions, LabelButtonBlock[] rightOptions)
		{
			this.leftOptions = leftOptions;
			this.rightOptions = rightOptions;
		}

		public void Show(Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			View.ButtonsLeft = leftOptions;
			View.ButtonsRight = rightOptions;

			ShowView(parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);
		}
	}
}