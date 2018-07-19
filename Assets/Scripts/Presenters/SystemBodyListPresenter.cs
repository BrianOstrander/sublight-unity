﻿using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SystemBodyListPresenter : Presenter<ISystemBodyListView>
	{
		GameModel model;
		SystemModel destination;

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

			var bodies = new List<LabelButtonBlock>();

			foreach (var body in destination.Bodies.Value)
			{
				bodies.Add(new LabelButtonBlock(body.Name, () => OnBodyButtonClick(body)));
			}

			View.BodyEntries = bodies.ToArray();

			View.DoneClick = OnDoneClick;
			ShowView(App.GameCanvasRoot);
		}

		#region Events
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

		void OnBodyButtonClick(BodyModel body)
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			var encounter = App.Encounters.AssignBestEncounter(model, destination, body);
			if (encounter != null) body.Encounter.Value = encounter.EncounterId;

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

		void OnDoneClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			// Temp Begin
			foreach (var body in destination.Bodies.Value)
			{
				var added = body.ResourcesCurrent;
				
				model.Ship.Value.Inventory.AllResources.Add(added);
				
				body.ResourcesAcquired.Add(added);
				
				App.Callbacks.DialogRequest(
					DialogRequest.Alert(
						"Acquired " + Strings.Rations(added.Rations) + " rations and " + Strings.Fuel(added.Fuel) + " fuel",
						done: OnAlertClosed
					)
				);
			}
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