﻿using System;
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
		ConversationLanguageBlock conversationLanguage;

		List<BustEdgeModel> initializationInfos = new List<BustEdgeModel>();

		BustEdgeModel lastFocus;
		BustEdgeModel lastFocusInitialization;
		ConversationInstanceModel lastConversationFocus;

		List<ConversationInstanceModel> conversationInstances = new List<ConversationInstanceModel>();

		ConversationPromptButtonsPresenter promptButtonsPresenter;
		ConversationButtonsPresenter primaryButtonsPresenter;

		protected override bool CanReset() { return false; } // View should be reset on the beginning of an encounter.

		protected override bool CanShow()
		{
			return model.Context.EncounterState.Current.Value.State == EncounterStateModel.States.Processing && lastFocus != null;
		}

		public BustPresenter(
			GameModel model,
			ConversationLanguageBlock conversationLanguage
		)
		{
			this.model = model;
			this.conversationLanguage = conversationLanguage;

			App.Callbacks.EncounterRequest += OnEncounterRequest;

			promptButtonsPresenter = new ConversationPromptButtonsPresenter();
			primaryButtonsPresenter = new ConversationButtonsPresenter(model);
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
					initializationInfos.Clear();
					break;
				case EncounterRequest.States.Handle:
					request.TryHandle<BustHandlerModel>(OnHandleBust);
					break;
				case EncounterRequest.States.PrepareComplete:
					lastFocus = null;
					lastFocusInitialization = null;
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
			BustEdgeModel focusEdge = null;
			BustEdgeModel focusEdgeInitialization = null;
			var initializeBlocks = new List<BustBlock>();
			var initializeConversationIds = new List<string>();

			Action onHaltingDone = handler.HaltingDone.Value;

			foreach (var entry in handler.Entries.Value)
			{
				switch (entry.Operation.Value)
				{
					case BustEdgeModel.Operations.Initialize:
						initializationInfos = initializationInfos.Where(i => i.BustId.Value != entry.BustId.Value).Append(entry).ToList();
						initializeBlocks.Add(OnInitializeInfoToBlock(entry.BustId.Value, entry.InitializeInfo.Value));
						initializeConversationIds.Add(entry.BustId.Value);
						break;
					case BustEdgeModel.Operations.Focus:
						if (focusEdge != null) hadMultipleFocuses = true;
						focusEdge = entry;
						break;
					default:
						Debug.LogError("Unrecognized Bust.Operation: " + entry.Operation.Value);
						break;
				}
			}

			if (hadMultipleFocuses) Debug.LogError("Bust EncounterLog " + handler.Log.Value.LogId.Value + " contained multiple focuses, the last one was used");

			View.InitializeBusts(initializeBlocks.ToArray());

			foreach (var bustId in initializeConversationIds) OnInitializeConversation(bustId);

			if (focusEdge == null)
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

			focusEdgeInitialization = initializationInfos.FirstOrDefault(i => i.BustId.Value == focusEdge.BustId.Value);

			lastFocus = focusEdge;
			lastFocusInitialization = focusEdgeInitialization;
			var newConversationFocus = conversationInstances.First(i => i.BustId.Value == focusEdge.BustId.Value);
			var oldConversationFocus = lastConversationFocus;

			if (lastConversationFocus != null && lastConversationFocus.BustId.Value != focusEdge.BustId.Value) lastConversationFocus.IsFocused.Value = false;
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
					focusEdge.BustId.Value,
					focusEdge.FocusInfo.Value.Instant,
					focusBustId => focusCompleted = true
				);
				primaryButtonsPresenter.Theme = focusEdgeInitialization.InitializeInfo.Value.Theme;
				lastConversationFocus.Show.Value(false);
				if (oldConversationFocus != null && !oldConversationFocus.IsClosed.Value()) oldConversationFocus.Close.Value(false);
			};

			SM.PushBlocking(onCallFocus, onHaltingCondition, "FocusingBust");
			SM.Push(onHaltingDone, "HaltingDone");
		}

		BustBlock OnInitializeInfoToBlock(string bustId, BustEdgeModel.InitializeBlock info)
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
				case BustEdgeModel.TransmissionStrengths.Hidden: result.TransmitionStrengthIndex = -1; break;
				case BustEdgeModel.TransmissionStrengths.Failed: result.TransmitionStrengthIndex = 0; break;
				case BustEdgeModel.TransmissionStrengths.Weak: result.TransmitionStrengthIndex = 1; break;
				case BustEdgeModel.TransmissionStrengths.Intermittent: result.TransmitionStrengthIndex = 2; break;
				case BustEdgeModel.TransmissionStrengths.Strong: result.TransmitionStrengthIndex = 3; break;
				default: Debug.LogError("Unrecognized TransmissionStrength: " + info.TransmitionStrengthIcon); break;
			}

			switch (info.AvatarType)
			{
				case BustEdgeModel.AvatarTypes.Static:
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
			new ConversationPresenter(model, conversationModel, conversationLanguage);

			conversationInstances.Add(conversationModel);
		}

		BustBlock OnInitializeInfoToBlockStaticAvatar(BustBlock block, BustEdgeModel.InitializeBlock info)
		{
			block.AvatarStaticIndex = info.AvatarStaticIndex;
			block.AvatarStaticTerminalTextVisible = info.AvatarStaticTerminalTextVisible;
			return block;
		}
 
		void OnPrompt(ConversationButtonBlock prompt)
		{
			promptButtonsPresenter.Theme = lastFocusInitialization.InitializeInfo.Value.Theme;
			promptButtonsPresenter.HandleButtons(prompt);
		}
	}
	#endregion
}