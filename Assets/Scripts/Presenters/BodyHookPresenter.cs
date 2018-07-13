using System.Linq;
using System.Collections.Generic;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class BodyHookPresenter : Presenter<IBodyHookView>
	{
		GameModel model;
		SystemModel system;
		BodyModel body;
		EncounterInfoModel encounter;

		public BodyHookPresenter(GameModel model)
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

			var current = body.ResourcesCurrent;

			View.Title = body.Name;
			View.Description = encounter == null ? "No encounter" : "Encounter: " + encounter.EncounterId.Value;
			View.Rations = current.Rations;
			View.Fuel = current.Fuel;
			View.LaunchEnabled = encounter == null ? false : model.Ship.Value.Inventory.GetTypes().AnyIntersectOrEmpty(encounter.ValidCrews.Value);
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
					if (bodyFocus.View != BodyFocusRequest.Views.BodyHook) goto default;
					system = model.Universe.Value.GetSystem(bodyFocus.System);
					body = system.GetBody(bodyFocus.Body);
					encounter = App.Encounters.AssignBestEncounter(model, system, body);
					Show();
					break;
				default:
					if (View.TransitionState == TransitionStates.Shown) CloseView();
					break;
			}
		}

		void OnBackClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			App.Callbacks.FocusRequest(
				new SystemBodiesFocusRequest(system.Position)
			);
		}

		void OnLaunchClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			UnityEngine.Debug.Log("lol launch click here");
			//App.Callbacks.FocusRequest(
			//	new SystemBodiesFocusRequest(system.Position)
			//);
		}
		#endregion

	}
}