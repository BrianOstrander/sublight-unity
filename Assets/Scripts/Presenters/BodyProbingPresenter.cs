using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class BodyProbingPresenter : Presenter<IBodyProbingView>
	{
		GameModel model;
		SystemModel system;
		BodyModel body;
		ProbeInventoryModel probe;
		float? probingRemaining;

		public BodyProbingPresenter(GameModel model)
		{
			this.model = model;

			App.Heartbeat.Update += OnUpdate;
			App.Callbacks.FocusRequest += OnFocus;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.FocusRequest -= OnFocus;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = "Probing " + body.Name;

			probingRemaining = null;

			switch(body.Status.Value)
			{
				case BodyStatus.NotProbed:
					View.Description = "Starting Probing...";
					probingRemaining = 0.25f;
					break;
				case BodyStatus.EncounterFound:
					View.Description = "Done Probing!\nAn anomaly was discovered!";
					View.IsDone = true;
					break;
				case BodyStatus.EncounterNotFound:
					View.Description = "Done Probing!\nNo anomalous readings.";
					View.IsDone = true;
					break;
			}

			View.DoneClick = OnDoneClick;

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		void OnUpdate(float delta)
		{
			if (!probingRemaining.HasValue) return;
			if (View.TransitionState != TransitionStates.Shown) return;
			if (App.Callbacks.LastPlayState.State != PlayState.States.Playing) return;

			probingRemaining = Mathf.Max(0f, probingRemaining.Value - delta);
			if (!Mathf.Approximately(0f, probingRemaining.Value)) return;

			probingRemaining = null;

			body.ProbeId.Value = probe.InstanceId;

			if (body.HasEncounter)
			{
				View.Description = "Done Probing!\nAn anomaly was discovered!";
				body.Status.Value = BodyStatus.EncounterFound;
			}
			else
			{
				View.Description = "Done Probing!\nNo anomalous readings.";
				body.Status.Value = BodyStatus.EncounterNotFound;
			}

			View.IsDone = true;
		}

		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Body:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var bodyFocus = focus as BodyFocusRequest;
					// We also only show up if our view is specified
					if (bodyFocus.View != BodyFocusRequest.Views.Probing) goto default;
					system = model.Universe.Value.GetSystem(bodyFocus.System);
					body = system.GetBody(bodyFocus.Body);
					probe = model.Ship.Value.GetInventoryFirstOrDefault<ProbeInventoryModel>(bodyFocus.Probe);
					Show();
					break;
				default:
					if (View.TransitionState == TransitionStates.Shown) CloseView();
					break;
			}
		}

		void OnDoneClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			// Temp Begin
			var rationsAdded = body.Rations - body.RationsAcquired;
			var fuelAdded = body.Fuel - body.FuelAcquired;
			model.Ship.Value.Rations.Value += rationsAdded;
			model.Ship.Value.Fuel.Value += fuelAdded;

			body.RationsAcquired.Value = body.Rations;
			body.FuelAcquired.Value = body.Fuel;

			App.Callbacks.DialogRequest(
				DialogRequest.Alert(
					"Acquired " + Strings.Rations(rationsAdded) + " rations and " + Strings.Fuel(fuelAdded) + " fuel", 
					done: OnAlertClosed
				)
			);
			// Temp End
		}

		void OnAlertClosed()
		{
			// Eventually this should just be in OnDone, once temp code is removed.
			switch (body.Status.Value)
			{
				case BodyStatus.EncounterNotFound:
					App.Callbacks.FocusRequest(
						new SystemBodiesFocusRequest(system.Position)
					);
					break;
				case BodyStatus.EncounterFound:
					Debug.Log("lol encounter found todo here");
					break;
			}
		}
		#endregion

	}
}