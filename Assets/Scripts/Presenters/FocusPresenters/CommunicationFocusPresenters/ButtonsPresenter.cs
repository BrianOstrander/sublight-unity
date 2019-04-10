using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public interface IButtonsPresenter : IPresenter
	{
		ConversationThemes Theme { set; }
		void HandleButtons(params ConversationButtonBlock[] buttons);
	}

	public abstract class ButtonsPresenter<V> : CommunicationFocusPresenter<V>, IButtonsPresenter
		where V : class, IConversationButtonsView
	{
		bool isProcessingButtons;

		protected override bool CanReset() { return !isProcessingButtons; }
		protected override bool CanShow() { return isProcessingButtons; }

		public ConversationThemes Theme { set; private get; }

		public void HandleButtons(params ConversationButtonBlock[] buttons)
		{
			OnHandleButtons(buttons);
		}

		#region Events
		void OnHandleButtons(params ConversationButtonBlock[] buttons)
		{
			isProcessingButtons = true;

			var selectedTheme = View.DefaultTheme;

			switch (Theme)
			{
				case ConversationThemes.Internal: selectedTheme = View.CrewTheme; break;
				case ConversationThemes.AwayTeam: selectedTheme = View.AwayTeamTheme; break;
				case ConversationThemes.Foreigner: selectedTheme = View.ForeignerTheme; break;
				case ConversationThemes.Downlink: selectedTheme = View.DownlinkTheme; break;
				default: Debug.LogError("Unrecognized Theme: " + Theme); break;
			}

			if (View.Visible) CloseView(true);
			View.Reset();

			View.Click = OnClick;
			View.SetButtons(selectedTheme, buttons);

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