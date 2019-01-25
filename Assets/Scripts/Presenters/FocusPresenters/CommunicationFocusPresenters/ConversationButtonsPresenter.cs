using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ConversationButtonsPresenter : CommunicationFocusPresenter<IConversationButtonsView>
	{
		GameModel model;

		bool isProcessingButtons;

		protected override bool CanReset() { return false; } // View should be reset on the beginning of an encounter.

		protected override bool CanShow()
		{
			return model.EncounterState.State.Value == EncounterStateModel.States.Processing && isProcessingButtons;
		}

		public ConversationButtonsPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}

		#region Events
		void OnEncounterRequest(EncounterRequest request)
		{
			switch (request.State)
			{
				case EncounterRequest.States.Request:
					View.Reset();
					break;
				case EncounterRequest.States.Handle:
					request.TryHandle<ButtonHandlerModel>(OnHandleButtons);
					break;
				case EncounterRequest.States.PrepareComplete:
					isProcessingButtons = false;
					if (View.Visible) CloseView();
					break;
			}
		}

		void OnHandleButtons(ButtonHandlerModel handler)
		{
			if (handler.Log.Value.Style.Value != ButtonEncounterLogModel.Styles.Conversation) return;

			isProcessingButtons = true;

			var style = handler.Log.Value.ConversationStyle.Value;
			var theme = View.DefaultTheme;

			switch (style.Theme)
			{
				case ButtonEncounterLogModel.ConversationStyleBlock.Themes.Crew: theme = View.CrewTheme; break;
				case ButtonEncounterLogModel.ConversationStyleBlock.Themes.AwayTeam: theme = View.AwayTeamTheme; break;
				case ButtonEncounterLogModel.ConversationStyleBlock.Themes.Foreigner: theme = View.ForeignerTheme; break;
				case ButtonEncounterLogModel.ConversationStyleBlock.Themes.Downlink: theme = View.DownlinkTheme; break;
				default: Debug.LogError("Unrecognized Theme: " + style.Theme); break;
			}

			var buttonBlocks = new List<ConversationButtonBlock>();

			foreach (var entry in handler.Buttons.Value)
			{
				var block = new ConversationButtonBlock();
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
			isProcessingButtons = false;
			if (click == null) Debug.LogError("Null clicks are unhandled");
			else click();
		}
		#endregion
	}
}