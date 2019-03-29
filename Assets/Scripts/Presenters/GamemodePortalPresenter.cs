using System.Linq;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class GamemodePortalPresenter : Presenter<IGamemodePortalView>, IPresenterCloseShowOptions
	{
		GamemodeInfoModel[] gamemodes;
		GamemodeBlock[] gamemodeBlocks;
		GamemodePortalLanguageBlock language;

		int selectedGamemode;

		public GamemodePortalPresenter(
			GamemodeInfoModel[] gamemodes,
			GamemodePortalLanguageBlock language
		)
		{
			this.gamemodes = gamemodes.OrderBy(g => g.OrderWeight.Value).ToArray();
			this.language = language;

			gamemodeBlocks = new GamemodeBlock[this.gamemodes.Length];

			for (var i = 0; i < gamemodeBlocks.Length; i++)
			{
				var currentGamemode = this.gamemodes[i];
				Debug.Log(currentGamemode.Name.Value);
				gamemodeBlocks[i] = new GamemodeBlock
				{
					Title = currentGamemode.Title.Value,
					SubTitle = currentGamemode.SubTitle.Value,
					Description = currentGamemode.Description.Value,
					StartText = currentGamemode.IsInDevelopment ? language.Locked.Value.Value : language.Start.Value.Value,
					LockState = currentGamemode.IsInDevelopment ? GamemodeBlock.LockStates.InDevelopment : GamemodeBlock.LockStates.Unlocked,
					Icon = currentGamemode.Icon
				};
			}

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
                gamemodeBlocks[selectedGamemode],
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

            selectedGamemode = (selectedGamemode + 1) % gamemodeBlocks.Length;

            View.SetGamemode(
                gamemodeBlocks[selectedGamemode],
                false,
                true
            );
        }

        void OnPreviousClick()
        {
            if (View.TransitionState != TransitionStates.Shown) return;

            selectedGamemode--;

            if (selectedGamemode < 0) selectedGamemode = gamemodeBlocks.Length + selectedGamemode;

            View.SetGamemode(
                gamemodeBlocks[selectedGamemode],
                false,
                false
            );
        }
        #endregion
    }
}