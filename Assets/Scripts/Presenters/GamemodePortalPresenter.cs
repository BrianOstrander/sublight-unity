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
		int? selectionIndexFirstPlayable;
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
				if (!selectionIndexFirstPlayable.HasValue && !currentGamemode.IsInDevelopment.Value) selectionIndexFirstPlayable = i;
			}

			if (gamemodes.Length == 0) Debug.LogError("No gamemodes were loaded");
			else if (!selectionIndexFirstPlayable.HasValue) Debug.LogError("No playable gamemodes are available");

			App.Callbacks.Escape += OnEscape;
			App.Callbacks.HoloColorRequest += OnHoloColorRequest;
			App.Heartbeat.Update += OnUpdate;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.Escape -= OnEscape;
			App.Callbacks.HoloColorRequest -= OnHoloColorRequest;
			App.Heartbeat.Update -= OnUpdate;
		}

		public void Show(
			Action<GamemodeInfoModel> selection,
			Action back,
			bool instant = false,
			bool resetIndex = true
		)
		{
			if (selection == null) throw new ArgumentNullException("selection");
			if (back == null) throw new ArgumentNullException("back");

			if (View.Visible) Debug.LogError("Trying to show gamemode portal when it's already visible, unpredictable behaviour may occur.");

			if (selectionIndexFirstPlayable.HasValue)
			{
				selectionIndex = Mathf.Clamp(selectionIndex, 0, gamemodes.Length - 1);
				if (resetIndex && gamemodes[selectionIndex].IsInDevelopment.Value) selectionIndex = selectionIndexFirstPlayable.Value;
			}

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

		protected virtual bool NotInteractable
		{
			get
			{
				return View.TransitionState != TransitionStates.Shown;
			}
		}

		#region Events
		void OnEscape()
		{
			if (NotInteractable) return;

			OnBackClick();
		}

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
			if (NotInteractable) return;

			View.Closed += OnClosedStart;
			CloseView();
        }

        void OnNextClick()
        {
			if (NotInteractable) return;

			selectionIndex = (selectionIndex + 1) % gamemodeBlocks.Length;

            View.SetGamemode(
                gamemodeBlocks[selectionIndex],
                false,
                true
            );
        }

        void OnPreviousClick()
        {
			if (NotInteractable) return;

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
			if (NotInteractable) return;

			View.Closed += OnClosedBack;
			CloseView();
		}

		void OnClosedStart()
		{
			var gamemodeSelected = gamemodes[selectionIndex];

			if (gamemodeSelected.IsInDevelopment.Value)
			{
				App.Callbacks.DialogRequest(
					DialogRequest.Confirm(
						language.UnavailableInDevelopment.Message,
						DialogStyles.Error,
						language.UnavailableInDevelopment.Title,
						() => Show(selection, back, resetIndex: false)
					)
				);
			}
			else selection(gamemodeSelected);
		}

		void OnClosedBack()
		{
			back();
		}
		#endregion
	}
}