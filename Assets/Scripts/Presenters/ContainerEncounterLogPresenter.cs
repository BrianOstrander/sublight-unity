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

		public ContainerEncounterLogPresenter(GameModel model, EncounterInfoModel infoModel)
		{
			this.model = model;
			this.infoModel = infoModel;
		}

		protected override void UnBind()
		{
			base.UnBind();

		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = infoModel.Name;

			View.Shown += OnShown;

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		void OnShown()
		{
			var beginning = infoModel.Logs.Beginning;
			if (beginning == null)
			{
				Debug.LogError("No beginning found for encounter " + infoModel.EncounterId.Value);
				return;
			}
			var nextLog = beginning;
			// TODO: This is probably dangerous and prone to loops and infinite
			// problems, but oh well!
			while (nextLog != null)
			{
				Func<GameModel, EncounterLogModel, IEntryEncounterLogPresenter> handler;
				if (LogHandlers.TryGetValue(nextLog.LogType, out handler))
				{
					var current = handler(model, nextLog);
					current.Show(View.EntryArea);
					entries.Add(current);
					var nextLogId = nextLog.NextLog;
					if (string.IsNullOrEmpty(nextLogId))
					{
						Debug.Log("Handle null next logs here!");
						break;
					}
					nextLog = infoModel.Logs.GetLogFirstOrDefault(nextLogId);
					if (nextLog == null)
					{
						Debug.LogError("Next log could not be found.");
						break;
					}
				}
				else
				{
					Debug.LogError("Unhandled LogType: " + nextLog.LogType);
					break;
				}
			}
		}
		#endregion
	}
}