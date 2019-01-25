using System;
using System.Linq;
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
			/*
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
			*/

			//View.AddToConversation(
			//	false,
			//	//shortIncoming
			//	//mediumIncoming
			//	//longIncoming,

			//	//shortOutgoing
			//	//mediumIncoming
			//	//longIncoming

			//	//shortIncoming,
			//	//mediumOutgoing,
			//	//longIncoming,
			//	//shortOutgoing

			//	new MessageConversationBlock { Type = ConversationTypes.MessageIncoming, Message = "First: Should appear on top" }
			//	//new MessageConversationBlock { Type = ConversationTypes.MessageIncoming, Message = "Second" },
			//	//new MessageConversationBlock { Type = ConversationTypes.MessageIncoming, Message = "Third" },
			//	//new MessageConversationBlock { Type = ConversationTypes.MessageIncoming, Message = "Fourth: Should appear on Bottom" }
			//);
			AddRandom();
		}

		void AddRandom()
		{
			var block = new MessageConversationBlock();
			block.Type = NumberDemon.DemonUtility.NextBool ? ConversationTypes.MessageIncoming : ConversationTypes.MessageOutgoing;

			var options = new string[]
			{
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
				"Cras eu nibh tincidunt, volutpat leo sed, vestibulum lacus.",
				"Duis et sem auctor, sollicitudin nisl in, hendrerit neque.",
				"Donec egestas erat ut lacus cursus, a ultrices massa efficitur.",
				"Cras vitae dui eu ipsum faucibus gravida eu ac est.",
				"Aliquam in leo ac risus vestibulum porta et nec leo.",
				"Phasellus vitae metus eu augue pretium tincidunt.",
				"Cras consequat eros nec varius placerat.",
				"Suspendisse vel leo ultricies, rutrum enim id, imperdiet neque.",
			};

			var chosen = string.Empty;

			for (var i = 0; i < NumberDemon.DemonUtility.GetNextInteger(1, 5); i++) chosen += options.Where(o => !chosen.Contains(o)).Random()+" ";

			block.Message = chosen;

			View.AddToConversation(false, block);

			App.Heartbeat.Wait(AddRandom, 2f);
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