using System;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipMapPresenter : Presenter<IShipMapView>
	{
		GameModel model;

		public ShipMapPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.StateChange += OnStateChange;
			model.Ship.Value.Position.Changed += OnShipPosition;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.StateChange -= OnStateChange;
			model.Ship.Value.Position.Changed -= OnShipPosition;
		}

		public void Show(Action done = null)
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = model.Ship.Value.Position;
			if (done != null) View.Shown += done;
			ShowView(instant: true);
		}

		#region Events
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End) CloseView(true);
		}

		void OnShipPosition(UniversePosition position)
		{
			View.UniversePosition = position;
		}
		#endregion
	}
}