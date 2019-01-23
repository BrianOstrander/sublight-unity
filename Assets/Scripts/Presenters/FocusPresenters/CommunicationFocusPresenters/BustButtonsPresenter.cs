using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class BustButtonsPresenter : CommunicationFocusPresenter<IBustButtonsView>
	{
		//GameModel model;

		public BustButtonsPresenter(GameModel model)
		{
			//this.model = model;

			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}

		protected override void OnUpdateEnabled()
		{
			/*
			View.SetButtons(
				View.DownlinkTheme,
				new BustButtonBlock
				{
					Message = "First Button",
					Used = false,
					Interactable = true,
					Click = () => Debug.Log("Clicked first")
				},
				new BustButtonBlock
				{
					Message = "Second Button",
					Used = true,
					Interactable = true,
					Click = () => Debug.Log("Clicked second")
				},
				new BustButtonBlock
				{
					Message = "Third Button",
					Used = false,
					Interactable = false,
					Click = () => Debug.Log("Clicked third")
				},
				new BustButtonBlock
				{
					Message = "Fourth Button",
					Used = true,
					Interactable = false,
					Click = () => Debug.Log("Clicked fourth")
				}
			);
			*/
		}

		#region Events
		void OnEncounterRequest(EncounterRequest request)
		{
			if (request.State == EncounterRequest.States.Handle) request.TryHandle<ButtonHandlerModel>(OnHandleButtons);
		}

		void OnHandleButtons(ButtonHandlerModel handler)
		{
			if (handler.Log.Value.Style.Value != ButtonEncounterLogModel.Styles.Bust) return;

			var style = handler.Log.Value.BustStyle.Value;
			var theme = View.DefaultTheme;

			switch (style.Theme)
			{
				case ButtonEncounterLogModel.BustStyleBlock.Themes.Crew: theme = View.CrewTheme; break;
				case ButtonEncounterLogModel.BustStyleBlock.Themes.AwayTeam: theme = View.AwayTeamTheme; break;
				case ButtonEncounterLogModel.BustStyleBlock.Themes.Foreigner: theme = View.ForeignerTheme; break;
				case ButtonEncounterLogModel.BustStyleBlock.Themes.Downlink: theme = View.DownlinkTheme; break;
				default: Debug.LogError("Unrecognized Theme: " + style.Theme); break;
			}

			var buttonBlocks = new List<BustButtonBlock>();

			foreach (var entry in handler.Buttons.Value)
			{
				var block = new BustButtonBlock();
				block.Message = entry.Message;
				block.Used = entry.Used;
				block.Interactable = entry.Interactable;
				block.Click = entry.Click;

				buttonBlocks.Add(block);
			}

			if (View.Visible) CloseView(true);
			View.Reset();

			View.Click = OnClick;
			View.SetButtons(theme, buttonBlocks.ToArray());

			ShowView();
		}

		void OnClick(Action click)
		{
			View.Closed += () => OnReadyForClick(click);

			CloseView();
		}

		void OnReadyForClick(Action click)
		{
			if (click == null) Debug.LogError("Null clicks are unhandled");
			else click();
		}
		#endregion
	}
}