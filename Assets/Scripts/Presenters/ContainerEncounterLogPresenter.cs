﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ContainerEncounterLogPresenter : Presenter<IContainerEncounterLogView>
	{
		static Dictionary<EncounterLogTypes, Func<GameModel, EncounterLogModel, IEntryEncounterLogPresenter>> LogHandlers = new Dictionary<EncounterLogTypes, Func<GameModel, EncounterLogModel, IEntryEncounterLogPresenter>> {
			{ EncounterLogTypes.Text, (gameModel, logModel) => new TextEncounterLogPresenter(gameModel, logModel as TextEncounterLogModel) }
		};

		GameModel model;
		EncounterInfoModel encounter;
		SystemModel system;
		BodyModel body;
		CrewInventoryModel crew;
		List<IEntryEncounterLogPresenter> entries = new List<IEntryEncounterLogPresenter>();

		EncounterLogModel nextLog;
		float? nextLogDelay;

		public ContainerEncounterLogPresenter(GameModel model)
		{
			this.model = model;

			App.Heartbeat.Update += OnUpdate;
			App.Callbacks.FocusRequest += OnFocus;
		}

		protected override void OnUnBind()
		{
			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.FocusRequest -= OnFocus;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = encounter.Name;
			View.DoneClick = OnDoneClick;
			View.NextClick = OnNextClick;
			View.Shown += OnShown;
			View.PrepareClose += OnPrepareClose;

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Encounter:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var encounterFocus = focus as EncounterFocusRequest;
					encounter = App.Encounters.GetEncounter(encounterFocus.EncounterId);
					system = model.Universe.Value.GetSystem(encounterFocus.System);
					body = system.GetBody(encounterFocus.Body);
					crew = model.Ship.Value.Inventory.GetInventoryFirstOrDefault<CrewInventoryModel>(encounterFocus.Crew);
					entries.Clear();
					Show();
					break;
				default:
					if (focus.State == FocusRequest.States.Active)
					{
						if (View.TransitionState == TransitionStates.Shown) CloseView();
					}
					break;
			}
		}

		void OnShown()
		{
			nextLog = encounter.Logs.Beginning;
			if (nextLog == null)
			{
				Debug.LogError("No beginning found for encounter " + encounter.EncounterId.Value);
				return;
			}
			nextLogDelay = 0f;
		}

		void OnPrepareClose()
		{
			// TODO: Make this nicer and not need to close instantly???
			foreach (var entry in entries)
			{
				entry.Close();
				App.P.UnRegister(entry);
			}
			entries.Clear();
		}

		void OnDoneClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			model.SetEncounterStatus(EncounterStatus.Completed(encounter.EncounterId));

			App.Callbacks.FocusRequest(
				new SystemBodiesFocusRequest(system.Position)
			);
		}

		void OnNextClick()
		{
			if (nextLogDelay.HasValue) nextLogDelay = 0f;
		}

		void OnUpdate(float delta)
		{
			if (!nextLogDelay.HasValue) return;
			if (View.TransitionState != TransitionStates.Shown) return;
			if (App.Callbacks.LastPlayState.State != PlayState.States.Playing) return;

			if (App.Preferences.EncounterLogsAutoNext.Value) nextLogDelay = Mathf.Max(0f, nextLogDelay.Value - delta);

			if (!Mathf.Approximately(0f, nextLogDelay.Value)) return;

			OnShowLog(nextLog);
		}

		void OnShowLog(EncounterLogModel logModel)
		{
			nextLog = null;
			nextLogDelay = null;
			
			Func<GameModel, EncounterLogModel, IEntryEncounterLogPresenter> handler;
			if (LogHandlers.TryGetValue(logModel.LogType, out handler))
			{
				var current = handler(model, logModel);
				current.Show(View.EntryArea, OnShownLog);
				entries.Add(current);

				// TODO: Unlock done button when end is reached.
				if (logModel.Ending.Value)
				{
					View.DoneEnabled = true;
					View.NextEnabled = false;
					return;
				}

				var nextLogId = logModel.NextLog;
				if (string.IsNullOrEmpty(nextLogId))
				{
					Debug.Log("Handle null next logs here!");
					return;
				}
				nextLog = encounter.Logs.GetLogFirstOrDefault(nextLogId);
				if (nextLog == null)
				{
					Debug.LogError("Next log could not be found.");
					return;
				}
				View.NextEnabled = true;
				nextLogDelay = logModel.TotalDuration;
			}
			else Debug.LogError("Unhandled LogType: " + logModel.LogType);
		}

		void OnShownLog()
		{
			View.Scroll = 0f;
		}
		#endregion
	}
}