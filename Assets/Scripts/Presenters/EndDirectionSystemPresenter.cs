using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class EndDirectionSystemPresenter : Presenter<IEndDirectionSystemView>
	{
		GameModel model;
		SystemModel system;

		public EndDirectionSystemPresenter(GameModel model)
		{
			this.model = model;
			system = model.Universe.Value.GetSystem(model.EndSystem);

			model.Ship.Value.Position.Changed += OnShipPosition;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.Ship.Value.Position.Changed -= OnShipPosition;
		}

		public void Show()
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = model.Ship.Value.Position;
			View.EndPosition = system.Position;

			ShowView(instant: true);
		}

		#region Events
		void OnShipPosition(UniversePosition position)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.UniversePosition = position;
		}
		#endregion
	}
}