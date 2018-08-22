using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class SystemBodyListPresenter : Presenter<ISystemBodyListView>
	{
		GameModel model;
		SystemModel destination;

		LabelButtonBlock? body;

		public SystemBodyListPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.FocusRequest += OnFocus;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.FocusRequest -= OnFocus;
		}

		void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = Strings.ArrivedIn(destination.Name.Value);

			body = null;
			App.Encounters.AssignBestEncounter(OnAssignBestEncounter, model, destination);
		}

		#region Events
		void OnAssignBestEncounter(AssignBestEncounter result)
		{
			if (result.Status != RequestStatus.Success || !result.EncounterAssigned)
			{
				OnAssignBestEncounterDone();
				return;
			}

			body = new LabelButtonBlock(result.Body == null ? "Non body encounter" : "Encounter on BodyId: "+result.Body.BodyId.Value, () => OnEncounterClick(result), true);

			OnAssignBestEncounterDone();
		}

		void OnAssignBestEncounterDone()
		{
			if (body.HasValue) View.BodyEntries = new LabelButtonBlock[] { body.Value };

			View.DoneClick = OnDoneClick;
			ShowView(App.GameCanvasRoot);
		}

		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.SystemBodies:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var systemBodiesFocus = focus as SystemBodiesFocusRequest;
					destination = model.Universe.Value.GetSystem(systemBodiesFocus.System);
					Show();
					break;
				default:
					if (View.TransitionState == TransitionStates.Shown) CloseView();
					break;
			}
		}

		void OnEncounterClick(AssignBestEncounter result)
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			switch (model.GetEncounterStatus(result.Encounter.EncounterId.Value).State)
			{
				case EncounterStatus.States.Unknown:
					App.Callbacks.DialogRequest(DialogRequest.Alert("Scanners detect no anomalies."));
					return;
				case EncounterStatus.States.Completed:
					var finalReport = model.GetFinalReport(result.System.EncounterId.Value);
					if (finalReport != null) App.Callbacks.DialogRequest(DialogRequest.Alert(finalReport.Summary, "Crew Report"));
					else App.Callbacks.DialogRequest(DialogRequest.Alert("Scanners detect no additional anomalies."));
					return;
				default:
					App.Callbacks.EncounterRequest(
						EncounterRequest.Request(
							model,
							result.Encounter.EncounterId,
							result.System.Position
						)
					);
					break;
			}
		}

		/*
		void OnBodyButtonClick(BodyModel body)
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			switch(model.GetEncounterStatus(body.Encounter).State)
			{
				case EncounterStatus.States.Unknown:
					App.Callbacks.DialogRequest(DialogRequest.Alert("Scanners detect no anomalies."));
					return;
				case EncounterStatus.States.Completed:
					var finalReport = model.GetFinalReport(body.Encounter);
					if (finalReport != null) App.Callbacks.DialogRequest(DialogRequest.Alert(finalReport.Summary, "Crew Report"));
					else App.Callbacks.DialogRequest(DialogRequest.Alert("Scanners detect no additional anomalies."));
					return;
				default:
					App.Callbacks.FocusRequest(
						BodyFocusRequest.BodyHook(destination.Position, body.BodyId)
					);
					break;
			}
		}
		*/

		void OnDoneClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			// Temp Begin
			var totalResources = ResourceInventoryModel.Zero;

			foreach (var body in destination.Bodies.Value)
			{
				var added = body.ResourcesCurrent;
				totalResources.Add(added);
				body.ResourcesAcquired.Add(added);
				
			}

			model.Ship.Value.Inventory.AllResources.Add(totalResources);

			/* This was getting annoying...
			App.Callbacks.DialogRequest(
				DialogRequest.Alert(
					"Acquired " + Strings.Rations(totalResources.Rations) + " rations and " + Strings.Fuel(totalResources.Fuel) + " fuel",
					done: OnAlertClosed
				)
			);
			*/
			OnAlertClosed();

			// Temp End
		}

		void OnAlertClosed()
		{
			App.Callbacks.FocusRequest(
				new SystemsFocusRequest(
					destination.Position.Value.SystemZero,
					destination.Position.Value
				)
			);
		}
		#endregion

	}
}