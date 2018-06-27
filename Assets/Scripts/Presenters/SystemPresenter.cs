using System.Linq;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
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
			model.Ship.Value.TravelRadius.Changed += OnTravelRadius;
			model.DestructionRadius.Changed += OnDestructionRadius;
			model.FocusedSectors.Changed += OnFocusedSectors;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Heartbeat.Update -= OnUpdate;
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
			if (App.Callbacks.LastTravelRequest.State != TravelRequest.States.Complete) return;
			if (isTravelable)
			{
				var travelTime = UniversePosition.TravelTime(model.Ship.Value.CurrentSystem.Value, system.Position.Value, model.Ship.Value.SpeedTotal.Value);

				var travel = new TravelRequest(
					TravelRequest.States.Request,
					model.Ship.Value.CurrentSystem,
					model.Ship.Value.CurrentSystem,
					system.Position,
					App.Callbacks.LastDayTimeDelta.Current,
					App.Callbacks.LastDayTimeDelta.Current + travelTime,
					0f
				);
				App.Callbacks.TravelRequest(travel);
			}
			else
			{
				App.Log("Too far to travel here");
			}
		}

		void OnTravelRadius(TravelRadius travelRadius)
		{
			var distance = UniversePosition.Distance(system.Position, model.Ship.Value.Position);
			isTravelable = distance < travelRadius.MaximumRadius;
			if (View.Visible) OnSystemState();
		}

		void OnDestructionRadius(float radius)
		{
			isDestroyed = UniversePosition.Distance(UniversePosition.Zero, system.Position) < radius;
			if (View.Visible) OnSystemState();
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
				UnBind();
			}
		}
		#endregion
	}
}