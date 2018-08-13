using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class EndDirectionSystemPresenter : Presenter<IEndDirectionSystemView>, IPresenterCloseShow
	{
		GameModel model;
		SystemModel system;

		public EndDirectionSystemPresenter(GameModel model)
		{
			this.model = model;
			system = model.Universe.Value.GetSystem(model.EndSystem);

			model.Ship.Value.Position.Changed += OnShipPosition;
		}

		protected override void OnUnBind()
		{
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

		public void Close()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView();
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