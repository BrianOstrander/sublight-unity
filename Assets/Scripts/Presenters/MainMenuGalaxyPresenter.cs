using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class MainMenuGalaxyPresenter : Presenter<IMainMenuGalaxyView>, IPresenterCloseShowOptions
	{

		GalaxyPreviewModel galaxy;

		public MainMenuGalaxyPresenter(GalaxyPreviewModel galaxy)
		{
			this.galaxy = galaxy;
		}

		public void Show(Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			View.GalaxyPreview = galaxy.Preview;

			ShowView(parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);
		}
	}
}