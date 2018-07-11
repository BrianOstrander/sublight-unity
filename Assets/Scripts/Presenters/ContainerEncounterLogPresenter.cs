using System;
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
		EncounterInfoModel infoModel;
		List<IEntryEncounterLogPresenter> entries = new List<IEntryEncounterLogPresenter>();

		EncounterLogModel nextLog;
		float? nextLogDelay;

		public ContainerEncounterLogPresenter(GameModel model, EncounterInfoModel infoModel)
		{
			this.model = model;
			this.infoModel = infoModel;

			App.Heartbeat.Update += OnUpdate;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Heartbeat.Update -= OnUpdate;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = infoModel.Name;
			View.DoneClick = OnDoneClick;
			View.Shown += OnShown;

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		void OnShown()
		{
			nextLog = infoModel.Logs.Beginning;
			if (nextLog == null)
			{
				Debug.LogError("No beginning found for encounter " + infoModel.EncounterId.Value);
				return;
			}
			nextLogDelay = 0f;
		}

		void OnDoneClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			//App.Callbacks.FocusRequest(
			//	new SystemBodiesFocusRequest(system.Position)
			//);
		}

		void OnUpdate(float delta)
		{
			if (!nextLogDelay.HasValue) return;
			if (View.TransitionState != TransitionStates.Shown) return;
			if (App.Callbacks.LastPlayState.State != PlayState.States.Playing) return;

			nextLogDelay = Mathf.Max(0f, nextLogDelay.Value - delta);
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
					return;
				}

				var nextLogId = logModel.NextLog;
				if (string.IsNullOrEmpty(nextLogId))
				{
					Debug.Log("Handle null next logs here!");
					return;
				}
				nextLog = infoModel.Logs.GetLogFirstOrDefault(nextLogId);
				if (nextLog == null)
				{
					Debug.LogError("Next log could not be found.");
					return;
				}
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