using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class FuelSliderView : CanvasView, IFuelSliderView
	{
		[SerializeField]
		TextMeshProUGUI fuelLabel;
		[SerializeField]
		TextMeshProUGUI fuelConsumptionLabel;
		[SerializeField]
		Slider fuelConsumptionSlider;

		float fuel;

		public float Fuel 
		{
			set 
			{
				fuel = value;
				fuelConsumptionSlider.maxValue = fuel;
				fuelLabel.text = fuel.ToString("F2");
			}
			private get { return fuel; }
		}
		public float FuelConsumption
		{
			set 
			{
                fuelConsumptionSlider.value = value;
				SetFuelConsumptionLabel(value);
			}
		}
		public Action<float> FuelConsumptionUpdate { set; private get; }

		public override void Reset()
		{
			base.Reset();

			FuelConsumptionUpdate = ActionExtensions.GetEmpty<float>();
			Fuel = 0f;
			FuelConsumption = 0f;
		}

		void SetFuelConsumptionLabel(float value)
		{
			fuelConsumptionLabel.text = value.ToString("F2");
		}

		#region Events
		public void OnFuelConsumptionUpdate(float value)
		{
			SetFuelConsumptionLabel(value);
			FuelConsumptionUpdate(value);
		}
  		#endregion
	}

	public interface IFuelSliderView : ICanvasView
	{
		float Fuel { set; }
		float FuelConsumption { set; }
		Action<float> FuelConsumptionUpdate { set; }
	}
}