using System;
using System.Linq;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class GamemodePortalPresenter : Presenter<IGamemodePortalView>
	{
		GamemodeInfoModel[] gamemodes;
		GamemodeBlock[] gamemodeBlocks;
		GamemodePortalLanguageBlock language;

		int selectionIndex;
		Action<GamemodeInfoModel> selection;
		Action back;

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
				gamemodeBlocks[i] = new GamemodeBlock
				{
					Title = currentGamemode.Title.Value,
					SubTitle = currentGamemode.SubTitle.Value,
					Description = currentGamemode.Description.Value,
					StartText = currentGamemode.IsInDevelopment ? language.Locked.Value.Value : language.Start.Value.Value,
					LockState = currentGamemode.IsInDevelopment ? GamemodeBlock.LockStates.InDevelopment : GamemodeBlock.LockStates.Unlocked,
					LockText = currentGamemode.IsInDevelopment ? language.InDevelopmentDescription.Value.Value : language.LockedDescription.Value.Value,
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

		public void Show(
			Action<GamemodeInfoModel> selection,
			Action back,
			bool instant = false
		)
		{
			if (selection == null) throw new ArgumentNullException("selection");
			if (back == null) throw new ArgumentNullException("back");

			if (View.Visible) Debug.LogError("Trying to show gamemode portal when it's already visible, unpredictable behaviour may occur.");

			this.selection = selection;
			this.back = back;

			View.Reset();

			View.HoloColor = App.Callbacks.LastHoloColorRequest.Color;
            View.StartClick = OnStartClick;
            View.NextClick = OnNextClick;
            View.PreviousClick = OnPreviousClick;

            View.SetGamemode(
                gamemodeBlocks[selectionIndex],
                true
            );

			View.BackText = language.Back.Value.Value;
			View.BackClick = OnBackClick;

			ShowView(instant: instant);
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

			View.Closed += OnClosedStart;
			CloseView();
        }

        void OnNextClick()
        {
            if (View.TransitionState != TransitionStates.Shown) return;

            selectionIndex = (selectionIndex + 1) % gamemodeBlocks.Length;

            View.SetGamemode(
                gamemodeBlocks[selectionIndex],
                false,
                true
            );
        }

        void OnPreviousClick()
        {
            if (View.TransitionState != TransitionStates.Shown) return;

            selectionIndex--;

            if (selectionIndex < 0) selectionIndex = gamemodeBlocks.Length + selectionIndex;

            View.SetGamemode(
                gamemodeBlocks[selectionIndex],
                false,
                false
            );
        }

		void OnBackClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			View.Closed += OnClosedBack;
			CloseView();
		}

		void OnClosedStart()
		{
			selection(gamemodes[selectionIndex]);
		}

		void OnClosedBack()
		{
			back();
		}
		#endregion
	}
}