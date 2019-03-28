using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GamemodePortalPresenter : Presenter<IGamemodePortalView>, IPresenterCloseShowOptions
	{
		public GamemodePortalPresenter()
		{
			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			App.Heartbeat.Update += OnUpdate;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Heartbeat.Update -= OnUpdate;
		}

		public void Show(Transform parent = null, bool instant = false)
		{
			if (View.Visible) return;

			View.Reset();

			View.HoloColor = App.Callbacks.LastHoloColorRequest.Color;
            View.StartClick = OnStartClick;
            View.NextClick = OnNextClick;
            View.PreviousClick = OnPreviousClick;

			ShowView(parent, instant);
		}

		public void Close(bool instant = false)
		{
			if (!View.Visible) return;

			CloseView(instant);
		}

		#region Events
		void OnHoloColorRequest(HoloColorRequest request)
		{
			View.HoloColor = request.Color;
		}

		void OnUpdate(float delta)
		{
			if (!View.Visible) return;

			View.PointerViewport = App.V.Camera.ScreenToViewportPoint(Input.mousePosition);
		}
        #endregion

        #region View Events
        void OnStartClick()
        {
            View.Closed += () => Show();
            Close();
        }

        void OnNextClick()
        {
            Debug.Log("Next Clicked");
        }

        void OnPreviousClick()
        {
            Debug.Log("Previous Clicked");
        }
        #endregion
    }
}