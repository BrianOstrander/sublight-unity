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

		protected override void OnUnBind()
		{
			App.Callbacks.FocusRequest -= OnFocus;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			var current = body.ResourcesCurrent;

			View.Title = body.Name;
			View.Rations = current.Rations;
			View.Fuel = current.Fuel;
			View.BackClick = OnBackClick;


			if (encounter == null) View.Description = "No encounter";
			else
			{
				var buttons = new List<LabelButtonBlock>();
				foreach (var crew in model.Ship.Value.Inventory.GetUsableInventory<CrewInventoryModel>())
				{
					if (!crew.IsExplorable(body)) continue;
					buttons.Add(new LabelButtonBlock(crew.Name, () => OnCrewClick(crew)));
				}
				View.CrewEntries = buttons.ToArray();

				var description = encounter.Hook.Value;
				if (buttons.Count() == 0) description += "\n\nNo crewed craft capable of exploring this encounter.";
				View.Description = description;
			}

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
		
		void OnCrewClick(CrewInventoryModel crew)
		{
			App.Callbacks.FocusRequest(
				EncounterFocusRequest.Encounter(
					encounter.EncounterId,
					system.Position,
					body.BodyId,
					crew.InventoryId
				)
			);
		}

		void OnBackClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			App.Callbacks.FocusRequest(
				new SystemBodiesFocusRequest(system.Position)
			);
		}
		#endregion

	}
}