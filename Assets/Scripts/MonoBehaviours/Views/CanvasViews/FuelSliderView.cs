﻿using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LunraGames.SubLight.Views
{
	public class FuelSliderView : CanvasView, IFuelSliderView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		TextMeshProUGUI rationsLabel;
		[SerializeField]
		TextMeshProUGUI fuelLabel;
		[SerializeField]
		TextMeshProUGUI fuelConsumptionLabel;
		[SerializeField]
		Slider fuelConsumptionSlider;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

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
		public Action SlotsClick { set; private get; }

		public float Rations { set { rationsLabel.text = value.ToString("F2"); } }

		public override void Reset()
		{
			base.Reset();

			Rations = 0f;
			FuelConsumptionUpdate = ActionExtensions.GetEmpty<float>();
			Fuel = 0f;
			FuelConsumption = 0f;
			SlotsClick = ActionExtensions.Empty;
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

		public void OnSlotsClick() { SlotsClick(); }
  		#endregion
	}

	public interface IFuelSliderView : ICanvasView
	{
		float Rations { set; }
		float Fuel { set; }
		float FuelConsumption { set; }
		Action<float> FuelConsumptionUpdate { set; }
		Action SlotsClick { set; }
	}
}