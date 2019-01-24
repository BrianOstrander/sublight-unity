using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ConversationPresenter : CommunicationFocusPresenter<IConversationView>
	{
		GameModel model;

		protected override bool CanReset() { return false; } // View should be reset on the beginning of an encounter.

		protected override bool CanShow()
		{
			return model.EncounterState.State.Value == EncounterStateModel.States.Processing;
		}

		public ConversationPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}

		protected override void OnUpdateEnabled()
		{
			//Debug.Log("lol running");

			var shortIncoming = new MessageConversationBlock();
			shortIncoming.Type = ConversationTypes.MessageIncoming;
			shortIncoming.Message = "Some short incoming message.";

			var mediumIncoming = new MessageConversationBlock();
			mediumIncoming.Type = ConversationTypes.MessageIncoming;
			mediumIncoming.Message = "Some medium incoming message.\nline";

			var longIncoming = new MessageConversationBlock();
			longIncoming.Type = ConversationTypes.MessageIncoming;
			longIncoming.Message = "Some long incoming message that should be broken up across multiple lines with some other text below it.\nline\nline\nline\nline";

			// --

			var shortOutgoing = new MessageConversationBlock();
			shortOutgoing.Type = ConversationTypes.MessageOutgoing;
			shortOutgoing.Message = "Some short outgoing message.";

			var mediumOutgoing = new MessageConversationBlock();
			mediumOutgoing.Type = ConversationTypes.MessageOutgoing;
			mediumOutgoing.Message = "Some medium outgoing message.\nline";

			var longOutgoing = new MessageConversationBlock();
			longOutgoing.Type = ConversationTypes.MessageOutgoing;
			longOutgoing.Message = "Some long outgoing message that should be broken up across multiple lines with some other text below it.\nline\nline\nline\nline";

			View.AddToConversation(
				true,
				//shortIncoming
				//mediumIncoming
				longIncoming,
			
				shortOutgoing
				//mediumIncoming
				//longIncoming
			);
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
					//request.TryHandle<ButtonHandlerModel>(OnHandleButtons);
					break;
				case EncounterRequest.States.Done:
					if (View.Visible) CloseView();
					break;
			}
		}
		#endregion
	}
}