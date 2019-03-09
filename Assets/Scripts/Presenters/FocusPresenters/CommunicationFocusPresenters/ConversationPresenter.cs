﻿using System;
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
		ConversationInstanceModel instanceModel;

		List<ConversationEntryModel> remainingEntries = new List<ConversationEntryModel>();
		Action onNoneRemaining;

		protected override bool CanReset() { return false; } // View should be reset on the beginning of an encounter.

		protected override bool CanShow()
		{
			return model.Context.EncounterState.Current.Value.State == EncounterStateModel.States.Processing && instanceModel.IsFocused.Value;
		}

		public ConversationPresenter(
			GameModel model,
			ConversationInstanceModel instanceModel
		)
		{
			this.model = model;
			this.instanceModel = instanceModel;

			this.instanceModel.Show.Value = OnShowInstance;
			this.instanceModel.Close.Value = OnCloseInstance;
			this.instanceModel.Destroy.Value = OnDestroyInstance;

			this.instanceModel.IsShown.Value = () => View.TransitionState == TransitionStates.Shown;
			this.instanceModel.IsClosed.Value = () => View.TransitionState == TransitionStates.Closed;
			this.instanceModel.IsDestroyed.Value = () => UnBinded;

			App.Callbacks.EncounterRequest += OnEncounterRequest;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.EncounterRequest -= OnEncounterRequest;
		}

		#region Events
		void OnShowInstance(bool instant)
		{
			if (View.TransitionState != TransitionStates.Shown) ShowView(instant: instant);
		}

		void OnCloseInstance(bool instant)
		{
			if (View.TransitionState != TransitionStates.Closed) CloseView(instant);
		}

		void OnDestroyInstance()
		{
			App.P.UnRegister(this);
		}

		void OnEncounterRequest(EncounterRequest request)
		{
			switch (request.State)
			{
				case EncounterRequest.States.Request:
					View.Reset();
					break;
				case EncounterRequest.States.Handle:
					if (instanceModel.IsFocused.Value) request.TryHandle<ConversationHandlerModel>(OnHandleConversation);
					break;
				case EncounterRequest.States.PrepareComplete:
					if (View.Visible) CloseView();
					break;
			}
		}

		void OnHandleConversation(ConversationHandlerModel handler)
		{
			if (remainingEntries.Any())
			{
				Debug.LogError("Handling conversation before remaining entries have been processed, unpredictable behaviour may occur");
				remainingEntries.Clear();
			}

			remainingEntries.AddRange(handler.Entries.Value);
			onNoneRemaining = handler.HaltingDone.Value;

			OnHandleRemaining();
		}

		void OnHandleRemaining()
		{
			if (remainingEntries.None())
			{
				var oldOnNoneRemaining = onNoneRemaining;
				onNoneRemaining = null;
				oldOnNoneRemaining();
				return;
			}

			var additions = new List<IConversationBlock>();
			var newRemainingEntries = new List<ConversationEntryModel>();
			Action onAdditionsDone = OnHandleRemaining;
			var isHalting = false;

			foreach (var entry in remainingEntries)
			{
				if (isHalting) newRemainingEntries.Add(entry);
				else
				{
					switch (entry.ConversationType.Value)
					{
						case ConversationTypes.MessageIncoming:
						case ConversationTypes.MessageOutgoing:
							isHalting = true;
							additions.Add(
								new MessageConversationBlock
								{
									Type = entry.ConversationType.Value,
									Message = entry.Message.Value
								}
							);
							break;
						case ConversationTypes.Prompt:
							isHalting = true;
							onAdditionsDone = () => OnHandlePrompt(entry);
							break;
						default:
							Debug.LogError("Unrecognized ConversationType: " + entry.ConversationType.Value + ", skipping...");
							break;
					}
				}
			}

			remainingEntries.Clear();
			remainingEntries.AddRange(newRemainingEntries);

			if (additions.None()) onAdditionsDone();
			else View.AddToConversation(false, onAdditionsDone, additions.ToArray());
		}

		void OnHandlePrompt(ConversationEntryModel entry)
		{
			instanceModel.OnPrompt.Value(
				entry.PromptInfo.Value.Style,
				entry.PromptInfo.Value.Theme,
				new ConversationButtonBlock
				{
					Message = entry.Message,
					Interactable = true,
					Click = OnHandleRemaining
				}
			);
		}
		#endregion
	}
}