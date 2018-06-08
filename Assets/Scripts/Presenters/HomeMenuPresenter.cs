using System;
using UnityEngine;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class HomeMenuPresenter : Presenter<IHomeMenuView>
	{
		public HomeMenuPresenter()
		{
			
		}

		protected override void UnBind()
		{
			base.UnBind();
		}

		public void Show(Action done)
		{
			if (View.Visible) return;
			View.Reset();
			View.StartClick = OnStartClick;
			View.Shown += done;
			ShowView(App.CanvasRoot, true);
		}

		#region Events
		void OnStartClick()
		{
			Debug.Log("Start clicked!");
		}
		#endregion

	}
}