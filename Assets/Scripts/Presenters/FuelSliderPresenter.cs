using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class FuelSliderPresenter : Presenter<IFuelSliderView>
	{
		GameModel model;

		public FuelSliderPresenter(GameModel model)
		{
			this.model = model;

			model.Ship.Value.Fuel.Changed += OnFuel;
			model.Ship.Value.FuelConsumption.Changed += OnFuelConsumption;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.Ship.Value.Fuel.Changed -= OnFuel;
			model.Ship.Value.FuelConsumption.Changed -= OnFuelConsumption;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Fuel = model.Ship.Value.Fuel;
			View.FuelConsumption = model.Ship.Value.FuelConsumption;
			View.FuelConsumptionUpdate = OnFuelConsumptionUpdate;

			ShowView(App.GameCanvasRoot, true);
		}

		#region Events
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