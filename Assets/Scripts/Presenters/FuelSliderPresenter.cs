using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class FuelSliderPresenter : Presenter<IFuelSliderView>, IPresenterCloseShow
	{
		GameModel model;

		public FuelSliderPresenter(GameModel model)
		{
			this.model = model;

			model.Ship.Value.Inventory.Resources.Fuel.Changed += OnFuel;
			model.Ship.Value.FuelConsumption.Changed += OnFuelConsumption;
			model.Ship.Value.Inventory.Resources.Rations.Changed += OnRations;
		}

		protected override void OnUnBind()
		{
			model.Ship.Value.Inventory.Resources.Fuel.Changed -= OnFuel;
			model.Ship.Value.FuelConsumption.Changed -= OnFuelConsumption;
			model.Ship.Value.Inventory.Resources.Rations.Changed -= OnRations;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			// Turn this back up to one, since it's annoying every time fuel runs out.
			if (1f <= model.Ship.Value.Inventory.Resources.Fuel && model.Ship.Value.FuelConsumption < 1f)
			{
				model.Ship.Value.FuelConsumption.Value = 1f;
			}

			View.Rations = model.Ship.Value.Inventory.Resources.Rations;
			View.Fuel = model.Ship.Value.Inventory.Resources.Fuel;
			View.FuelConsumption = model.Ship.Value.FuelConsumption;
			View.FuelConsumptionUpdate = OnFuelConsumptionUpdate;

			ShowView(App.GameCanvasRoot, true);
		}

		public void Close()
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			CloseView();
		}

		#region Events
		void OnRations(float rations)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.Rations = rations;
		}

		void OnFuel(float fuel)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.Fuel = fuel;
		}

		void OnFuelConsumption(float fuelConsumption)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.FuelConsumption = fuelConsumption;
		}

		void OnFuelConsumptionUpdate(float fuelConsumption)
		{
			model.Ship.Value.FuelConsumption.Value = fuelConsumption;
		}
		#endregion
	}
}