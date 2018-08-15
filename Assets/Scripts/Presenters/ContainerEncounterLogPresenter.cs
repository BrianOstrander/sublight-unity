using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
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
		//CrewInventoryModel crew;
		KeyValueListener keyValues;
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
					//crew = model.Ship.Value.Inventory.GetInventoryFirstOrDefault<CrewInventoryModel>(encounterFocus.Crew);
					keyValues = new KeyValueListener(KeyValueTargets.Encounter, encounterFocus.KeyValues, App.KeyValues);
					keyValues.Register();
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
				View.DoneEnabled = true;
				View.NextEnabled = false;
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

			CloseView();
			App.Callbacks.KeyValueRequest(KeyValueRequest.Get(KeyValueTargets.Encounter, EncounterKeys.Summary, OnGetFinalReport));
		}

		void OnGetFinalReport(KeyValueResult<string> result)
		{
			var summary = result.Status == RequestStatus.Success ? result.Value : "Nothing to report.";

			var finalReport = new FinalReportModel();
			finalReport.Encounter.Value = encounter.EncounterId;
			finalReport.System.Value = system.Position;
			finalReport.Body.Value = body.BodyId;
			finalReport.Summary.Value = summary;
			model.AddFinalReport(finalReport);

			App.Encounters.GetEncounterInteraction(encounter.EncounterId).TimesCompleted.Value++;

			App.Callbacks.DialogRequest(DialogRequest.Alert(summary, "Crew Report", OnAlertDone));
		}

		void OnAlertDone()
		{
			model.SetEncounterStatus(EncounterStatus.Completed(encounter.EncounterId));
			keyValues.UnRegister();

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

			// Some logic may be halting, so we have a done action for them to
			// call when they're finished.
			Action linearDone = () => OnHandledLog(logModel, logModel.NextLog);
			Action<string> nonLinearDone = nextLog => OnHandledLog(logModel, nextLog);

			switch(logModel.LogType)
			{
				case EncounterLogTypes.Text:
					OnPresentedLog(logModel);
					break;
				case EncounterLogTypes.KeyValue:
					OnKeyValueLog(logModel as KeyValueEncounterLogModel, linearDone);
					break;
				case EncounterLogTypes.Inventory:
					OnInventoryLog(logModel as InventoryEncounterLogModel, linearDone);
					break;
				case EncounterLogTypes.Switch:
					OnSwitchLog(logModel as SwitchEncounterLogModel, nonLinearDone);
					break;
				case EncounterLogTypes.Button:
					OnButtonLog(logModel as ButtonEncounterLogModel, nonLinearDone);
					break;
				default:
					Debug.LogError("Unrecognized LogType: " + logModel.LogType + ", skipping...");
					linearDone();
					break;
			}
		}

		void OnPresentedLog(EncounterLogModel logModel)
		{
			Func<GameModel, EncounterLogModel, IEntryEncounterLogPresenter> handler;
			if (LogHandlers.TryGetValue(logModel.LogType, out handler))
			{
				var current = handler(model, logModel);
				current.Show(View.EntryArea, OnShownLog);
				entries.Add(current);

				OnHandledLog(logModel, logModel.NextLog);
			}
			else Debug.LogError("Unrecognized LogType: " + logModel.LogType);
		}

		void OnKeyValueLog(KeyValueEncounterLogModel logModel, Action done)
		{
			var total = logModel.Operations.Value.Length;

			if (total == 0)
			{
				done();
				return;
			}

			var progress = 0;

			foreach (var entry in logModel.Operations.Value)
			{
				switch(entry.Operation)
				{
					case KeyValueOperations.SetString:
						var setString = entry as SetStringOperationModel;
						App.Callbacks.KeyValueRequest(
							KeyValueRequest.Set(
								entry.Target.Value,
								entry.Key.Value,
								setString.Value.Value,
								result => OnKeyValueLogDone(result, total, ref progress, done)
							)
						);
						break;
					case KeyValueOperations.SetBoolean:
						var setBoolean = entry as SetBooleanOperationModel;
						App.Callbacks.KeyValueRequest(
							KeyValueRequest.Set(
								entry.Target.Value,
								entry.Key.Value,
								setBoolean.Value.Value,
								result => OnKeyValueLogDone(result, total, ref progress, done)
							)
						);
						break;
					default:
						Debug.LogError("Unrecognized KeyValueType: " + entry.Operation);
						done();
						return;
				}
			}
		}

		void OnKeyValueLogDone<T>(KeyValueResult<T> result, int total, ref int progress, Action done) where T : IConvertible
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Setting " + result.TargetKey + " = " + result.Value + " returned with status: " + result.Status + " and error:\n" + result.Error);
				Debug.LogWarning("Continuing after this failure may result in unpredictable behaviour.");
			}
			progress++;
			if (total == progress) done();
		}

		void OnInventoryLog(InventoryEncounterLogModel logModel, Action done)
		{
			var total = logModel.Operations.Value.Length;

			if (total == 0)
			{
				done();
				return;
			}

			var progress = 0;

			foreach (var entry in logModel.Operations.Value)
			{
				switch (entry.Operation)
				{
					case InventoryOperations.AddResources:
						var addResource = entry as AddResourceOperationModel;
						var currentResources = model.Ship.Value.Inventory.AllResources.Duplicate;
						model.Ship.Value.Inventory.AllResources.Assign(currentResources.Add(addResource.Value).ClampNegatives());
						OnInventoryLogDone(total, ref progress, done);
						break;
					case InventoryOperations.AddInstance:
						var addInstance = entry as AddInstanceOperationModel;
						App.InventoryReferences.CreateInstance(
							addInstance.InventoryId,
							InventoryReferenceContext.Current(model),
							result => OnInventoryLogCreateInstance(result, total, ref progress, done)
						);
						break;
					default:
						Debug.LogError("Unrecognized InventoryOperation: " + entry.Operation);
						done();
						return;
				}
			}
		}

		void OnInventoryLogCreateInstance(InventoryReferenceRequest<InventoryModel> result, int total, ref int progress, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Creating instance from reference returned with status: " + result.Status + " and error:\n" + result.Error);
				Debug.LogWarning("Continuing after this failure may result in unpredictable behaviour.");
			}
			else
			{
				model.Ship.Value.Inventory.Add(result.Instance);
			}

			OnInventoryLogDone(total, ref progress, done);
		}

		void OnInventoryLogDone(int total, ref int progress, Action done)
		{
			progress++;
			if (total == progress) done();
		}

		void OnSwitchLog(SwitchEncounterLogModel logModel, Action<string> done)
		{
			var switches = logModel.Switches.Value.Where(e => !e.Ignore.Value && !string.IsNullOrEmpty(e.NextLogId.Value)).OrderBy(e => e.Index.Value).ToList();

			OnSwitchLogFilter(
				null,
				null,
				switches,
				(status, result) => OnSwitchLogDone(status, result, logModel, done)
			);
		}

		void OnSwitchLogFilter(
			bool? result, 
			string resultId, 
			List<SwitchEdgeModel> remaining, 
			Action<RequestStatus, string> done
		)
		{
			if (result.HasValue && result.Value)
			{
				done(RequestStatus.Success, resultId);
				return;
			}

			if (!remaining.Any())
			{
				done(RequestStatus.Failure, null);
				return;
			}

			var next = remaining.First();
			var nextId = next.NextLogId.Value;
			remaining.RemoveAt(0);

			App.ValueFilter.Filter(
				filterResult => OnSwitchLogFilter(filterResult, nextId, remaining, done),
				next.Filtering,
				model
			);
		}

		void OnSwitchLogDone(RequestStatus status, string nextLogId, SwitchEncounterLogModel logModel, Action<string> done)
		{
			if (status == RequestStatus.Success) done(nextLogId);
			else done(logModel.NextLog);
		}

		void OnButtonLog(ButtonEncounterLogModel logModel, Action<string> done)
		{
			var buttons = logModel.Buttons.Value.Where(e => !e.Ignore.Value && !string.IsNullOrEmpty(e.NextLogId.Value)).OrderBy(e => e.Index.Value).ToList();

			Action<RequestStatus, List<ButtonLogBlock>> filteringDone = (status, filtered) => OnButtonLogDone(status, filtered, logModel, done);

			OnButtonLogFilter(
				null,
				buttons,
				new List<ButtonLogBlock>(),
				done,
				filteringDone
			);
		}

		void OnButtonLogFilter(
			ButtonLogBlock? result,
			List<ButtonEdgeModel> remaining,
			List<ButtonLogBlock> filtered,
			Action<string> done,
			Action<RequestStatus, List<ButtonLogBlock>> filteringDone
		)
		{
			if (result.HasValue) filtered.Add(result.Value);

			if (!remaining.Any())
			{
				// No remaining to filter.
				if (filtered.Where(f => f.Interactable).Any()) filteringDone(RequestStatus.Success, filtered); // There are interactable buttons.
				else filteringDone(RequestStatus.Failure, null); // There are no interactable buttons.
				return;
			}

			Action<ButtonLogBlock?> nextDone = filterResult => OnButtonLogFilter(filterResult, remaining, filtered, done, filteringDone);
			var next = remaining.First();
			remaining.RemoveAt(0);
			var possibleResult = new ButtonLogBlock(
				next.Message.Value,
				false,
				true,
				() => done(next.NextLogId.Value)
			);

			if (next.AutoDisableEnabled.Value)
			{
				// When this button is pressed, it gets disabled, so we have to check.
				App.Callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						next.AutoDisabledKey,
						kvResult => OnButtonLogAutoDisabled(kvResult, next, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto disabled check.
				OnButtonLogAutoDisabled(
					null,
					next,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogAutoDisabled(
			KeyValueResult<bool>? kvResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled check, so see the result.
				if (kvResult.Value.Value)
				{
					// It has been auto disabled.
					nextDone(null);
					return;
				}
			}

			if (edge.AutoDisableInteractions.Value)
			{
				// When this button is pressed, interactions get disabled, so we have to check.
				App.Callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						edge.AutoDisabledInteractionsKey,
						interactionKvResult => OnButtonLogAutoDisabledInteractions(interactionKvResult, edge, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto disable interactions check.
				OnButtonLogAutoDisabledInteractions(
					null,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogAutoDisabledInteractions(
			KeyValueResult<bool>? kvResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled interaction check, so set the result.
				possibleResult.Interactable = !kvResult.Value.Value;
			}

			if (!edge.NotAutoUsed.Value)
			{
				// When this button is pressed, it gets marked as used, so we have to check to see if that happened.
				App.Callbacks.KeyValueRequest(
					KeyValueRequest.Get(
						KeyValueTargets.Encounter,
						edge.AutoUsedKey,
						autoUsedKvResult => OnButtonLogAutoUsed(autoUsedKvResult, edge, possibleResult, nextDone)
					)
				);
			}
			else
			{
				// Bypass the auto used check.
				OnButtonLogAutoUsed(
					null,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogAutoUsed(
			KeyValueResult<bool>? kvResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (kvResult.HasValue)
			{
				// We actually did a disabled interaction check, so set the result.
				possibleResult.Used = kvResult.Value.Value;
			}

			App.ValueFilter.Filter(
				filterResult => OnButtonLogEnabledFiltering(filterResult, edge, possibleResult, nextDone),
				edge.EnabledFiltering,
				model
			);
		}

		void OnButtonLogEnabledFiltering(
			bool filteringResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			if (!filteringResult)
			{
				// This button isn't enabled.
				nextDone(null);
				return;
			}

			if (possibleResult.Interactable)
			{
				// It hasn't automatically been disabled, so we check the filter.
				App.ValueFilter.Filter(
					filterResult => OnButtonLogInteractableFiltering(filterResult, edge, possibleResult, nextDone),
					edge.InteractableFiltering,
					model
				);
			}
			else
			{
				// Already not interactable, so bypass the filter.
				OnButtonLogInteractableFiltering(
					false,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogInteractableFiltering(
			bool filteringResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			possibleResult.Interactable &= filteringResult;

			if (!possibleResult.Used)
			{
				// Hasn't been auto marked as used, so we have to check the filter.
				App.ValueFilter.Filter(
					filterResult => OnButtonLogUsedFiltering(filterResult, edge, possibleResult, nextDone),
					edge.UsedFiltering,
					model
				);
			}
			else
			{
				// Auto using made it used, so we bypass the filter.
				OnButtonLogUsedFiltering(
					true,
					edge,
					possibleResult,
					nextDone
				);
			}
		}

		void OnButtonLogUsedFiltering(
			bool filteringResult,
			ButtonEdgeModel edge,
			ButtonLogBlock possibleResult,
			Action<ButtonLogBlock?> nextDone
		)
		{
			possibleResult.Used |= filteringResult;

			nextDone(possibleResult);
		}

		void OnButtonLogDone(RequestStatus status, List<ButtonLogBlock> buttons, ButtonEncounterLogModel logModel, Action<string> done)
		{
			if (status != RequestStatus.Success)
			{
				// No enabled and interactable buttons found.
				done(logModel.NextLog);
				return;
			}

			Debug.Log("TODO: spawning a button presenter!");

			//var current = handler(model, logModel);
			//current.Show(View.EntryArea, OnShownLog);
			//entries.Add(current);

			//OnHandledLog(logModel, logModel.NextLog);
		}

		void OnHandledLog(EncounterLogModel logModel, string nextLogId)
		{
			// TODO: Unlock done button when end is reached.
			if (logModel.Ending.Value)
			{
				View.DoneEnabled = true;
				View.NextEnabled = false;
				return;
			}

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

		void OnShownLog()
		{
			View.Scroll = 0f;
		}
		#endregion
	}
}