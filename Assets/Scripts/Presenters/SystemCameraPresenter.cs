using System;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SystemCameraPresenter : Presenter<ISystemCameraView>
	{
		public SystemCameraPresenter()
		{
			App.Callbacks.SystemCameraRequest += OnSystemCameraRequest;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.SystemCameraRequest -= OnSystemCameraRequest;
		}

		public void Show(Action done)
		{
			if (View.Visible) return;
			View.Reset();
			View.Shown += done;
			ShowView(instant: true);
		}

		#region Events
		void OnSystemCameraRequest(SystemCameraRequest request)
		{
			
		}
  		#endregion
	}
}