﻿using System.Collections.Generic;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class BodyProbeDetailPresenter : Presenter<IBodyProbeDetailView>
	{
		GameModel model;
		SystemModel system;
		BodyModel body;
		ProbeInventoryModel probe;

		public BodyProbeDetailPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.FocusRequest += OnFocus;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.FocusRequest -= OnFocus;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = probe.Name;
			View.Description = probe.Description;
			View.BackClick = OnBackClick;
			View.LaunchClick = OnLaunchClick;

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Body:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var bodyFocus = focus as BodyFocusRequest;
					// We also only show up if our view is specified
					if (bodyFocus.View != BodyFocusRequest.Views.ProbeDetail) goto default;
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

		void OnLaunchClick()
		{
			UnityEngine.Debug.Log("launchin' " + probe.Name);
			// Temp Begin
			var rationsAdded = body.Rations - body.RationsAcquired;
			var fuelAdded = body.Fuel - body.FuelAcquired;
			model.Ship.Value.Rations.Value += rationsAdded;
			model.Ship.Value.Fuel.Value += fuelAdded;

			body.RationsAcquired.Value = body.Rations;
			body.FuelAcquired.Value = body.Fuel;

			App.Callbacks.DialogRequest(
				DialogRequest.Alert("Acquired " + Strings.Rations(rationsAdded) + " rations and " + Strings.Fuel(fuelAdded) + " fuel")
			);
			// Temp End
		}

		void OnBackClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			App.Callbacks.FocusRequest(
				BodyFocusRequest.ProbeList(system.Position, body.BodyId)
			);
		}
		#endregion

	}
}