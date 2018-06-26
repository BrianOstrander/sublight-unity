using System.Linq;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SystemPresenter : Presenter<ISystemView>
	{
		GameModel gameModel;
		SystemModel model;

		bool isTravelable;
		bool isDestroyed;

		public SystemPresenter(GameModel gameModel, SystemModel model)
		{
			this.gameModel = gameModel;
			this.model = model;
			SetView(App.V.Get<ISystemView>(v => v.SystemType == model.SystemType));

			App.Callbacks.TravelRadiusChange += OnTravelRadiusChange;
			gameModel.DestructionRadius.Changed += OnDestructionRadius;
			gameModel.FocusedSectors.Changed += OnFocusedSectors;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.TravelRadiusChange -= OnTravelRadiusChange;
			gameModel.DestructionRadius.Changed -= OnDestructionRadius;
			gameModel.FocusedSectors.Changed -= OnFocusedSectors;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.UniversePosition = model.Position;
			View.Highlight = OnHighlight;
			View.Click = OnClick;
			OnTravelRadiusChange(App.Callbacks.LastTravelRadiusChange);
			OnSystemState();

			ShowView(instant: true);
		}

		#region Events
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
						if (App.Callbacks.LastSystemHighlight.System != model) return;
						break;
				}
			}
			App.Callbacks.SystemHighlight(new SystemHighlight(state, model));
		}

		void OnClick()
		{
			if (App.Callbacks.LastTravelRequest.State != TravelRequest.States.Complete) return;
			if (isTravelable)
			{
				var travelTime = UniversePosition.TravelTime(gameModel.Ship.Value.CurrentSystem.Value, model.Position.Value, gameModel.Ship.Value.Speed.Value);

				var travel = new TravelRequest(
					TravelRequest.States.Request,
					gameModel.Ship.Value.CurrentSystem,
					gameModel.Ship.Value.CurrentSystem,
					model.Position,
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

		void OnTravelRadiusChange(TravelRadiusChange travelRadiusChange)
		{
			var distance = UniversePosition.Distance(model.Position, travelRadiusChange.Origin);
			isTravelable = distance < travelRadiusChange.TravelRadius.MaximumRadius;
			if (View.Visible) OnSystemState();
		}

		void OnDestructionRadius(float radius)
		{
			isDestroyed = UniversePosition.Distance(UniversePosition.Zero, model.Position) < radius;
			if (View.Visible) OnSystemState();
		}

		void OnSystemState()
		{
			if (isDestroyed) View.SystemState = SystemStates.Destroyed;
			else if (gameModel.Ship.Value.Position.Value == model.Position.Value) View.SystemState = SystemStates.Current;
			else if (isTravelable) View.SystemState = SystemStates.InRange;
			else View.SystemState = SystemStates.OutOfRange;
		}

		void OnFocusedSectors(UniversePosition[] positions)
		{
			if (!positions.Contains(model.Position.Value.SystemZero))
			{
				CloseView(true);
				UnBind();
			}
		}
		#endregion
	}
}