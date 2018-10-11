using System.Linq;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class SystemPresenter : Presenter<ISystemView>
	{
		GameModel model;
		SystemModel system;

		bool isTravelable;
		bool isDestroyed;

		bool updatedThisFrame;

		public SystemPresenter(GameModel model, SystemModel system)
		{
			this.model = model;
			this.system = system;
			SetView(App.V.Get<ISystemView>(v => v.SystemType == system.SystemType));

			App.Heartbeat.Update += OnUpdate;
			App.Callbacks.FocusRequest += OnFocus;
			model.Ship.Value.TravelRadius.Changed += OnTravelRadius;
			model.DestructionRadius.Changed += OnDestructionRadius;
			model.FocusedSectors.Changed += OnFocusedSectors;
		}

		protected override void OnUnBind()
		{
			App.Heartbeat.Update -= OnUpdate;
			App.Callbacks.FocusRequest -= OnFocus;
			model.Ship.Value.TravelRadius.Changed -= OnTravelRadius;
			model.DestructionRadius.Changed -= OnDestructionRadius;
			model.FocusedSectors.Changed -= OnFocusedSectors;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.UniversePosition = system.Position;
			View.Highlight = OnHighlight;
			View.Click = OnClick;
			OnTravelRadius(model.Ship.Value.TravelRadius);
			OnDestructionRadius(model.DestructionRadius);
			OnSystemState();

			ShowView(instant: true);
		}

		#region Events
		void OnUpdate(float delta)
		{
			updatedThisFrame = false;
		}

		void OnFocus(FocusRequest focus)
		{
			switch(focus.Focus)
			{
				case FocusRequest.Focuses.Systems:
					if (focus.State == FocusRequest.States.Complete) Show();
					break;
				default:
					if (View.TransitionState == TransitionStates.Shown) CloseView();
					break;
			}
			
		}

		void OnHighlight(bool highlighted)
		{
			var state = SystemHighlight.States.End;
			if (highlighted)
			{
				switch (App.Callbacks.LastSystemHighlight.State)
				{
					case SystemHighlight.States.Unknown:
					case SystemHighlight.States.End:
						state = SystemHighlight.States.Begin;
						break;
					case SystemHighlight.States.Begin:
					case SystemHighlight.States.Change:
						state = SystemHighlight.States.Change;
						break;
				}
			}
			else
			{
				switch (App.Callbacks.LastSystemHighlight.State)
				{
					case SystemHighlight.States.Change:
						if (App.Callbacks.LastSystemHighlight.System != system) return;
						break;
				}
			}
			App.Callbacks.SystemHighlight(new SystemHighlight(state, system));
		}

		void OnClick()
		{
			var travelRequest = model.TravelRequest.Value;

			switch(travelRequest.State)
			{
				case TravelRequest.States.Complete:
					if (system.Position == travelRequest.Destination) OnClickToBodies();
					else OnClickToTravel();
					break;
			}
		}

		void OnClickToBodies()
		{
			App.Callbacks.FocusRequest(
				new SystemBodiesFocusRequest(
					system.Position
				)
			);
		}

		void OnClickToTravel()
		{
			if (isTravelable)
			{
				if (model.Ship.Value.Inventory.HasUnused)
				{
					App.Callbacks.DialogRequest(
						DialogRequest.CancelDenyConfirm(
							LanguageStringModel.Override("There are items or resources without slots, you'll lose them forever if you choose to continue."),
							LanguageStringModel.Override("Unused Items"),
							denyText: LanguageStringModel.Override("Show Me"),
							confirmText: LanguageStringModel.Override("Continue"),
							done: OnUnusedDialog
						)
					);
				}
				else OnInitiateTravel();
			}
			else
			{
				App.Log("Too far to travel here");
			}
		}

		void OnUnusedDialog(RequestStatus status)
		{
			switch(status)
			{
				case RequestStatus.Cancel: return;
				case RequestStatus.Failure:
					App.Callbacks.FocusRequest(
						ShipFocusRequest.SlotEditor()
					);
					return;
			}

			App.Callbacks.ClearInventoryRequest(ClearInventoryRequest.Request());

			OnInitiateTravel();
		}

		void OnInitiateTravel()
		{
			var travelTime = UniversePosition.TravelTime(model.Ship.Value.CurrentSystem.Value, system.Position.Value, model.Ship.Value.CurrentSpeed.Value);

			var travel = new TravelRequest(
				TravelRequest.States.Request,
				model.Ship.Value.CurrentSystem,
				model.Ship.Value.CurrentSystem,
				system.Position,
				App.Callbacks.LastDayTimeDelta.Current,
				App.Callbacks.LastDayTimeDelta.Current + travelTime,
				model.Ship.Value.FuelConsumption,
				0f
			);
			model.TravelRequest.Value = travel;
		}

		void OnTravelRadius(TravelRadius travelRadius)
		{
			var distance = UniversePosition.Distance(system.Position, model.Ship.Value.Position);
			isTravelable = distance < travelRadius.MaximumRadius;
			if (View.TransitionState == TransitionStates.Shown) OnSystemState();
		}

		void OnDestructionRadius(float radius)
		{
			isDestroyed = UniversePosition.Distance(UniversePosition.Zero, system.Position) < radius;
			if (View.TransitionState == TransitionStates.Shown) OnSystemState();
		}

		void OnSystemState()
		{
			if (updatedThisFrame) return;
			// TODO: Add more efficient checking of state to rule out unnecesary updates.
			if (isDestroyed) View.SystemState = SystemStates.Destroyed;
			else if (model.Ship.Value.Position.Value == system.Position.Value) View.SystemState = SystemStates.Current;
			else if (isTravelable) View.SystemState = SystemStates.InRange;
			else View.SystemState = SystemStates.OutOfRange;
		}

		void OnFocusedSectors(UniversePosition[] positions)
		{
			if (!positions.Contains(system.Position.Value.SystemZero))
			{
				CloseView(true);
				App.P.UnRegister(this);
			}
		}
		#endregion
	}
}