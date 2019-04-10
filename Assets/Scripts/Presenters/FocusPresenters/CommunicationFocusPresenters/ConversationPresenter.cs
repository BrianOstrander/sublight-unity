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
		struct ConversationQueueEntry
		{
			public enum Types
			{
				Unknown = 0,
				UnProcessed = 10,
				Processed = 20
			}

			public Types Type;
			public ConversationEntryModel UnProcessedEntry;
			public MessageConversationBlock ProcessedBlock;

			public ConversationQueueEntry(ConversationEntryModel unProcessedEntry)
			{
				Type = Types.UnProcessed;
				UnProcessedEntry = unProcessedEntry;
				ProcessedBlock = default(MessageConversationBlock);
			}

			public ConversationQueueEntry(MessageConversationBlock processedBlock)
			{
				Type = Types.Processed;
				UnProcessedEntry = null;
				ProcessedBlock = processedBlock;
			}
		}

		GameModel model;
		ConversationInstanceModel instanceModel;
		ConversationLanguageBlock language;

		ConversationHandlerModel currentHandler;
		List<ConversationQueueEntry> entryQueue = new List<ConversationQueueEntry>();
		Action onEntryQueueEmpty;

		protected override bool CanReset() { return false; } // View should be reset on the beginning of an encounter.

		protected override bool CanShow()
		{
			return model.Context.EncounterState.Current.Value.State == EncounterStateModel.States.Processing && instanceModel.IsFocused.Value;
		}

		public ConversationPresenter(
			GameModel model,
			ConversationInstanceModel instanceModel,
			ConversationLanguageBlock language
		)
		{
			this.model = model;
			this.instanceModel = instanceModel;
			this.language = language;

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
			if (entryQueue.Any())
			{
				Debug.LogError("Handling conversation before remaining entries have been processed, unpredictable behaviour may occur");
				entryQueue.Clear();
			}

			currentHandler = handler;
			entryQueue.AddRange(handler.Entries.Value.Select(e => new ConversationQueueEntry(e)));
			onEntryQueueEmpty = handler.HaltingDone.Value;

			OnHandleEntryQueue();
		}

		void OnHandleEntryQueue()
		{
			if (entryQueue.None())
			{
				var oldOnEntryQueueEmpty = onEntryQueueEmpty;
				onEntryQueueEmpty = null;
				oldOnEntryQueueEmpty();
				return;
			}

			var additions = new List<IConversationBlock>();
			var newEntryQueue = new List<ConversationQueueEntry>();
			Action onAdditionsDone = OnHandleEntryQueue;
			var isHalting = false;

			foreach (var currentEntry in entryQueue)
			{
				if (isHalting) newEntryQueue.Add(currentEntry);
				else
				{
					switch (currentEntry.Type)
					{
						case ConversationQueueEntry.Types.UnProcessed:
							switch (currentEntry.UnProcessedEntry.ConversationType.Value)
							{
								case ConversationTypes.MessageIncoming:
								case ConversationTypes.MessageOutgoing:
									isHalting = true;
									additions.Add(
										new MessageConversationBlock
										{
											Type = currentEntry.UnProcessedEntry.ConversationType.Value,
											Message = currentEntry.UnProcessedEntry.Message.Value
										}
									);
									break;
								case ConversationTypes.Prompt:
									isHalting = true;
									onAdditionsDone = () => OnHandlePrompt(currentEntry.UnProcessedEntry);
									break;
								default:
									Debug.LogError("Unrecognized ConversationType: " + currentEntry.UnProcessedEntry.ConversationType.Value + ", skipping...");
									break;
							}
							break;
						case ConversationQueueEntry.Types.Processed:
							isHalting = true;
							additions.Add(currentEntry.ProcessedBlock);
							break;
						default:
							Debug.LogError("Unrecognized queue entry: " + currentEntry.Type+", skipping...");
							break;
					}
				}
			}

			entryQueue.Clear();
			entryQueue.AddRange(newEntryQueue);

			if (additions.None()) onAdditionsDone();
			else View.AddToConversation(false, onAdditionsDone, additions.ToArray());
		}

		void OnHandlePrompt(ConversationEntryModel entry)
		{
			var promptBlock = new ConversationButtonBlock
			{
				Interactable = true,
				Click = () => OnHandlePromptClick(entry)
			};

			switch (entry.PromptInfo.Value.Behaviour)
			{
				case ConversationButtonPromptBehaviours.PrintOverride:
				case ConversationButtonPromptBehaviours.PrintMessage:
				case ConversationButtonPromptBehaviours.ButtonOnly:
					promptBlock.Message = entry.Message;
					break;
				case ConversationButtonPromptBehaviours.Continue:
					promptBlock.Message = language.ContinuePrompt.Value;
					break;
				default:
					Debug.LogError("Unrecognized PromptBehaviour: " + entry.PromptInfo.Value.Behaviour);
					break;
			}

			instanceModel.OnPrompt.Value(
				promptBlock
			);
		}

		void OnHandlePromptClick(ConversationEntryModel entry)
		{
			switch (entry.PromptInfo.Value.Behaviour)
			{
				case ConversationButtonPromptBehaviours.ButtonOnly:
				case ConversationButtonPromptBehaviours.Continue:
					break;
				case ConversationButtonPromptBehaviours.PrintMessage:
					entryQueue.Insert(
						0,
						new ConversationQueueEntry(
							new MessageConversationBlock
							{
								Type = ConversationTypes.MessageOutgoing,
								Message = entry.Message.Value
							}
						)
					);
					break;
				case ConversationButtonPromptBehaviours.PrintOverride:
					entryQueue.Insert(
						0,
						new ConversationQueueEntry(
							new MessageConversationBlock
							{
								Type = ConversationTypes.MessageOutgoing,
								Message = entry.PromptInfo.Value.MessageOverride
							}
						)
					);
					break;
				default:
					Debug.Log("Unrecognized PromptBehaviour: " + entry.PromptInfo.Value.Behaviour+", skipping...");
					break;
			}
			OnHandleEntryQueue();
		}
		#endregion
	}
}