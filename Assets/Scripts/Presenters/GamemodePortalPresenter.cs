using UnityEngine;

using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GamemodePortalPresenter : Presenter<IGamemodePortalView>, IPresenterCloseShowOptions
	{
        static GamemodeBlock[] allGamemodes =
        {
            new GamemodeBlock
            {
                Title = "Game",
                SubTitle = "A New Home",
                Description = "An interstellar ark with a thousand refugees has left Sol. You are tasked with finding a hospitable planet, one with an Earth-like climate. However, your resources are running low and your interstellar ark is falling apart.",
                StartText = "Start",
                LockState = GamemodeBlock.LockStates.Unlocked
            },
            new GamemodeBlock
            {
                Title = "Game",
                SubTitle = "Outrun the Void",
                Description = "Earth is gone, and the wake of its destruction is expanding outwards, towards your interstellar ark. Humanity’s only chance for survival is to flee from one star system to another, gathering vital resources along the way. The critical choices you make will have consequences for generations to come.",
                StartText = "Start",
                LockState = GamemodeBlock.LockStates.InDevelopment
            },
            new GamemodeBlock
            {
                Title = "Game",
                SubTitle = "Marathon",
                Description = "The grim black reaches of space beckon your people. This is a voyage of discovery, endurance, and ultimately... failure. Try to keep your interstellar ark alive for as long as possible, and see how far you can go!",
                StartText = "Start",
                LockState = GamemodeBlock.LockStates.InDevelopment
            }
        };

        int selectedGamemode;

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

            View.SetGamemode(
                allGamemodes[selectedGamemode],
                true
            );

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
            if (View.TransitionState != TransitionStates.Shown) return;

            View.Closed += () => Show();
            Close();
        }

        void OnNextClick()
        {
            if (View.TransitionState != TransitionStates.Shown) return;

            selectedGamemode = (selectedGamemode + 1) % allGamemodes.Length;

            View.SetGamemode(
                allGamemodes[selectedGamemode],
                false,
                true
            );
        }

        void OnPreviousClick()
        {
            if (View.TransitionState != TransitionStates.Shown) return;

            selectedGamemode--;

            if (selectedGamemode < 0) selectedGamemode = allGamemodes.Length + selectedGamemode;

            View.SetGamemode(
                allGamemodes[selectedGamemode],
                false,
                false
            );
        }
        #endregion
    }
}