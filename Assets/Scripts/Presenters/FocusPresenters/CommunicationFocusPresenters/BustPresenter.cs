using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class BustPresenter : CommunicationFocusPresenter<IBustView>
	{
		GameModel model;

		BustEntryModel lastFocus;
		ConversationInstanceModel lastConversationFocus;

		List<ConversationInstanceModel> conversationInstances = new List<ConversationInstanceModel>();

		List<IButtonsPresenter> buttonPresenters;

		protected override bool CanReset() { return false; } // View should be reset on the beginning of an encounter.

		protected override bool CanShow()
		{
			return model.EncounterState.Current.Value.State == EncounterStateModel.States.Processing && lastFocus != null;
		}

		public BustPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.EncounterRequest += OnEncounterRequest;

			buttonPresenters = new List<IButtonsPresenter>
			{
				new ConversationPromptButtonsPresenter()
			};
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();

			App.Callbacks.EncounterRequest -= OnEncounterRequest;

			foreach (var presenter in buttonPresenters) App.P.UnRegister(presenter);
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
					request.TryHandle<BustHandlerModel>(OnHandleBust);
					break;
				case EncounterRequest.States.PrepareComplete:
					lastFocus = null;
					lastConversationFocus = null;

					foreach (var conversationInstance in conversationInstances)
					{
						SM.PushBlocking(
							conversationInstance.Destroy,
							conversationInstance.IsDestroyed,
							"DestroyingConversation."+conversationInstance.BustId.Value,
							request.SynchronizedId
						);
					}

					conversationInstances.Clear();

					if (View.Visible)
					{
						CloseView();
						SM.PushBlocking(
							() => CloseView(),
							() => View.TransitionState == TransitionStates.Closed,
							"CloseView",
							request.SynchronizedId
						);
					}
					break;
			}
		}

		void OnHandleBust(BustHandlerModel handler)
		{
			var hadMultipleFocuses = false;
			BustEntryModel focusEntry = null;
			var initializeBlocks = new List<BustBlock>();
			var initializeConversationIds = new List<string>();

			Action onHaltingDone = handler.HaltingDone.Value;

			foreach (var entry in handler.Entries.Value)
			{
				switch (entry.BustEvent.Value)
				{
					case BustEntryModel.Events.Initialize:
						initializeBlocks.Add(OnInitializeInfoToBlock(entry.BustId.Value, entry.InitializeInfo.Value));
						initializeConversationIds.Add(entry.BustId.Value);
						break;
					case BustEntryModel.Events.Focus:
						if (focusEntry != null) hadMultipleFocuses = true;
						focusEntry = entry;
						break;
					default:
						Debug.LogError("Unrecognized BustEvent: " + entry.BustEvent.Value);
						break;
				}
			}

			if (hadMultipleFocuses) Debug.LogError("Bust EncounterLog " + handler.Log.Value.LogId.Value + " contained multiple focuses, the last one was used");

			View.InitializeBusts(initializeBlocks.ToArray());

			foreach (var bustId in initializeConversationIds) OnInitializeConversation(bustId);

			if (focusEntry == null)
			{
				onHaltingDone();
				return;
			}

			if (model.ToolbarSelection.Value != ToolbarSelections.Communication)
			{
				Debug.LogError("Cannot focus the bust view when player is not on the Communication focus!");
				return;
			}

			if (View.TransitionState != TransitionStates.Shown) ShowView(instant: true);

			lastFocus = focusEntry;
			var newConversationFocus = conversationInstances.First(i => i.BustId.Value == focusEntry.BustId.Value);
			var oldConversationFocus = lastConversationFocus;

			if (lastConversationFocus != null && lastConversationFocus.BustId.Value != focusEntry.BustId.Value) lastConversationFocus.IsFocused.Value = false;
			else oldConversationFocus = null;

			lastConversationFocus = newConversationFocus;
			lastConversationFocus.IsFocused.Value = true;

			var focusCompleted = false;
			Func<bool> onHaltingCondition = () =>
			{
				return focusCompleted && newConversationFocus.IsShown.Value() && (oldConversationFocus == null || oldConversationFocus.IsClosed.Value());
			};

			Action onCallFocus = () =>
			{
				View.FocusBust(
					focusEntry.BustId.Value,
					focusEntry.FocusInfo.Value.Instant,
					focusBustId => focusCompleted = true
				);
				lastConversationFocus.Show.Value(false);
				if (oldConversationFocus != null && !oldConversationFocus.IsClosed.Value()) oldConversationFocus.Close.Value(false);
			};

			SM.PushBlocking(onCallFocus, onHaltingCondition, "FocusingBust");
			SM.Push(onHaltingDone, "HaltingDone");
		}

		BustBlock OnInitializeInfoToBlock(string bustId, BustEntryModel.InitializeBlock info)
		{
			var result = new BustBlock
			{
				BustId = bustId,

				TitleSource = info.TitleSource,
				TitleClassification = info.TitleClassification,

				TransmitionType = info.TransmitionType,
				TransmitionStrength = info.TransmitionStrength,

				PlacardName = info.PlacardName,
				PlacardDescription = info.PlacardDescription,
			};

			switch (info.TransmitionStrengthIcon)
			{
				case BustEntryModel.TransmissionStrengths.Hidden: result.TransmitionStrengthIndex = -1; break;
				case BustEntryModel.TransmissionStrengths.Failed: result.TransmitionStrengthIndex = 0; break;
				case BustEntryModel.TransmissionStrengths.Weak: result.TransmitionStrengthIndex = 1; break;
				case BustEntryModel.TransmissionStrengths.Intermittent: result.TransmitionStrengthIndex = 2; break;
				case BustEntryModel.TransmissionStrengths.Strong: result.TransmitionStrengthIndex = 3; break;
				default: Debug.LogError("Unrecognized TransmissionStrength: " + info.TransmitionStrengthIcon); break;
			}

			switch (info.AvatarType)
			{
				case BustEntryModel.AvatarTypes.Static:
					result = OnInitializeInfoToBlockStaticAvatar(result, info);
					break;
				default:
					Debug.LogError("Unrecognized AvatarType: " + info.AvatarType);
					break;
			}

			return result;
		}

		void OnInitializeConversation(string bustId)
		{
			if (conversationInstances.Any(i => i.BustId.Value == bustId)) return;
			var conversationModel = new ConversationInstanceModel();
			conversationModel.BustId.Value = bustId;
			conversationModel.OnPrompt.Value = OnPrompt;
			new ConversationPresenter(model, conversationModel);

			conversationInstances.Add(conversationModel);
		}

		BustBlock OnInitializeInfoToBlockStaticAvatar(BustBlock block, BustEntryModel.InitializeBlock info)
		{
			block.AvatarStaticIndex = info.AvatarStaticIndex;
			return block;
		}
 
		void OnPrompt(ConversationButtonStyles style, ConversationThemes theme, ConversationButtonBlock prompt)
		{
			var buttonsPresenter = buttonPresenters.FirstOrDefault(p => p.Style == style);

			if (buttonsPresenter == null)
			{
				Debug.LogError("Unrecognized Style: " + style + ", skipping prompt");
				if (prompt.Click != null) prompt.Click();
				return;
			}
			
			buttonsPresenter.HandleButtons(theme, prompt);
		}
	}
	#endregion
}