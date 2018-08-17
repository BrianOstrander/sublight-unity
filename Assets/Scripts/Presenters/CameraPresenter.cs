using System;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class CameraPresenter : Presenter<ICameraView>
	{
		public void Show(Action done)
		{
			if (View.Visible) return;
			View.Reset();
			View.Shown += done;
			ShowView(instant: true);
		}
	}
}