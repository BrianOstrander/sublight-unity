using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class BustPresenter : CommunicationFocusPresenter<IBustView>
	{
		GameModel model;

		public BustPresenter(GameModel model)
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
			//OnLabel(labelProperty.Value);
			//View.PushOpacity(() => scaleModel.Opacity.Value);
			//View.PushOpacity(() => model.GridScaleOpacity.Value);

			/*
			View.InitializeBusts(
				new BustBlock
				{
					BustId = "lol",
					TitleSource = "Some",
					TitleClassification = "Ark",
					TransmitionType = "Transmission",
					TransmitionStrength = "Strong",
					TransmitionStrengthIndex = 0,
					PlacardName = "S. Cap",
					PlacardDescription = "Captain",
					AvatarStaticIndex = 1
				},
				new BustBlock
				{
					BustId = "lol2",
					TitleSource = "Other",
					TitleClassification = "Ark",
					TransmitionType = "Transmission",
					TransmitionStrength = "Weak",
					TransmitionStrengthIndex = 2,
					PlacardName = "S. Caap",
					PlacardDescription = "Captain'",
					AvatarStaticIndex = 0
				}
			);

			View.FocusBust("lol", true);

			App.Heartbeat.Wait(() => View.FocusBust("lol2", onFocus: id => Debug.Log("Id "+id+" shown")), 0.5f);
			App.Heartbeat.Wait(() => View.FocusBust("lol", true, id => Debug.Log("Id " + id + " shown instantly")), 1f);
			*/
		}

		#region Events
		void OnEncounterRequest(EncounterRequest request)
		{
			if (request.State == EncounterRequest.States.Handle) request.TryHandle<BustHandlerModel>(OnHandleBust);
		}

		void OnHandleBust(BustHandlerModel handler)
		{
			var hadMultipleFocuses = false;
			BustEntryModel focusEntry = null;
			var initializeBlocks = new List<BustBlock>();

			Action onHaltingDone = () =>
			{
				if (handler.HasHaltingEvents.Value && handler.HaltingDone.Value != null) handler.HaltingDone.Value();
			};

			foreach (var entry in handler.Entries.Value)
			{
				switch (entry.BustEvent.Value)
				{
					case BustEntryModel.Events.Initialize: initializeBlocks.Add(OnInitializeInfoToBlock(entry.BustId.Value, entry.InitializeInfo.Value)); break;
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

			if (focusEntry == null)
			{
				onHaltingDone();
				return;
			}

			var focusCompleted = false;
			Func<bool> onHaltingCondition = () => focusCompleted;
			Action onCallFocus = () => View.FocusBust(focusEntry.BustId.Value, focusEntry.FocusInfo.Value.Instant, focusBustId => focusCompleted = true);

			App.SM.PushBlocking(onCallFocus, onHaltingCondition);
			App.SM.Push(onHaltingDone);
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

		BustBlock OnInitializeInfoToBlockStaticAvatar(BustBlock block, BustEntryModel.InitializeBlock info)
		{
			block.AvatarStaticIndex = info.AvatarStaticIndex;
			return block;
		}
	}
	#endregion
}