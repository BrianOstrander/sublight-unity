using System.Collections.Generic;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ConversationButtonsPresenter : ButtonsPresenter<IConversationButtonsView>
	{
		GameModel model;

		protected override bool CanReset() { return false; } // View should be reset on the beginning of an encounter.

		protected override bool CanShow()
		{
			return base.CanShow() && model.Context.EncounterState.Current.Value.State == EncounterStateModel.States.Processing;
		}

		public override ConversationButtonStyles Style { get { return ConversationButtonStyles.Conversation; } }

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
					request.TryHandle<ButtonHandlerModel>(OnConversationHandleButtons);
					break;
				case EncounterRequest.States.PrepareComplete:
					if (View.Visible) CloseView();
					break;
			}
		}

		void OnConversationHandleButtons(ButtonHandlerModel handler)
		{
			if (handler.Log.Value.Style.Value != ConversationButtonStyles.Conversation) return;

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

			HandleButtons(handler.Log.Value.ConversationStyle.Value.Theme, buttonBlocks.ToArray());
		}
		#endregion
	}
}