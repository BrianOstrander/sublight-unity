using System;

using UnityEngine;
using UnityEngine.Events;

using TMPro;

namespace LunraGames.SpaceFarm.Views
{
	public class ShipSlotsView : CanvasView, IShipSlotsView
	{
		[SerializeField]
		TextMeshProUGUI titleLabel;
		[SerializeField]
		TextMeshProUGUI descriptionLabel;

		[SerializeField]
		TextMeshProUGUI maxRationsLabel;
		[SerializeField]
		TextMeshProUGUI maxFuelLabel;
		[SerializeField]
		TextMeshProUGUI maxSpeedLabel;
		[SerializeField]
		TextMeshProUGUI speedLabel;
		[SerializeField]
		TextMeshProUGUI rationGenerationLabel;
		[SerializeField]
		TextMeshProUGUI fuelGenerationLabel;
		[SerializeField]
		TextMeshProUGUI rationGenerationCapLabel;
		[SerializeField]
		TextMeshProUGUI fuelGenerationCapLabel;
		[SerializeField]
		TextMeshProUGUI rationConsumptionLabel;

		[SerializeField]
		ShipSlotLeaf slotEntryPrefab;
		[SerializeField]
		GameObject slotEntryArea;

		[SerializeField]
		ShipModuleLeaf moduleEntryPrefab;
		[SerializeField]
		GameObject moduleEntryArea;

		[SerializeField]
		Color entryNormalColor = Color.white;
		[SerializeField]
		Color entrySelectedColor = Color.white;

		public string Title { set { titleLabel.text = value ?? string.Empty; } }
		public string Description { set { descriptionLabel.text = value ?? string.Empty; } }

		public float MaxRations { set { maxRationsLabel.text = Strings.Rations(value); } }
		public float MaxFuel { set { maxFuelLabel.text = Strings.Fuel(value); } }
		public float MaxSpeed { set { maxSpeedLabel.text = Strings.Speed(value); } }

		public float Speed { set { speedLabel.text = Strings.Speed(value); } }

		public float RationGeneration { set { rationGenerationLabel.text = Strings.Rations(value); } }
		public float FuelGeneration { set { fuelGenerationLabel.text = Strings.Fuel(value); } }
		public float RationGenerationCap { set { rationGenerationCapLabel.text = Strings.Rations(value); } }
		public float FuelGenerationCap { set { fuelGenerationCapLabel.text = Strings.Fuel(value); } }

		public float RationConsumption { set { rationConsumptionLabel.text = Strings.Rations(value); } }

		public ShipSlotBlock[] SlotEntries { set { SetEntries(slotEntryArea, slotEntryPrefab, value); } }
		public ShipModuleBlock[] ModuleEntries { set { SetEntries(moduleEntryArea, moduleEntryPrefab, value); } }

		public Action DoneClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			slotEntryPrefab.gameObject.SetActive(false);
			moduleEntryPrefab.gameObject.SetActive(false);

			Title = string.Empty;
			Description = string.Empty;
			MaxRations = 0f;
			MaxFuel = 0f;
			MaxSpeed = 0f;
			Speed = 0f;
			RationGeneration = 0f;
			FuelGeneration = 0f;
			RationGenerationCap = 0f;
			FuelGenerationCap = 0f;
			RationConsumption = 0f;

			SlotEntries = null;
			ModuleEntries = null;

			DoneClick = ActionExtensions.Empty;
		}

		void SetEntries(GameObject root, ShipSlotLeaf prefab, params ShipSlotBlock[] entries)
		{
			root.transform.ClearChildren<ShipSlotLeaf>();
			if (entries == null) return;
			foreach (var entry in entries)
			{
				var instance = root.InstantiateChild(prefab, setActive: true);
				instance.SlotLabel.text = entry.SlotName ?? string.Empty;
				instance.TypeLabel.text = entry.TypeName ?? string.Empty;
				instance.Background.color = entry.IsSelected ? entrySelectedColor : entryNormalColor;
				instance.ButtonLabel.text = entry.ItemName ?? string.Empty;
				instance.Button.OnClick.AddListener(new UnityAction(entry.Click ?? ActionExtensions.Empty));
			}
		}

		void SetEntries(GameObject root, ShipModuleLeaf prefab, params ShipModuleBlock[] entries)
		{
			root.transform.ClearChildren<ShipModuleLeaf>();
			if (entries == null) return;
			foreach (var entry in entries)
			{
				var instance = root.InstantiateChild(prefab, setActive: true);
				instance.HeaderLabel.text = entry.Name ?? string.Empty;
				instance.DescriptionLabel.text = entry.Description ?? string.Empty;
				instance.Background.color = entry.CurrentlySlotted ? entrySelectedColor : entryNormalColor;

				if (entry.CurrentlySlotted)
				{
					instance.AssignedLabel.text = "[IN USE - CURRENT]";
					instance.AssignedLabel.color = Color.green;
				}
				else if (entry.IsSlotted)
				{
					instance.AssignedLabel.text = "[IN USE - ELSEWHERE]";
					instance.AssignedLabel.color = Color.yellow;
				}
				else
				{
					instance.AssignedLabel.text = "[AVAILABLE]";
					instance.AssignedLabel.color = Color.white;
				}

				instance.AssignButtonLabel.text = entry.ButtonText ?? string.Empty;
				instance.AssignButton.OnClick.AddListener(new UnityAction(entry.AssignClick ?? ActionExtensions.Empty));
				instance.AssignButton.interactable = !entry.CurrentlySlotted;

				instance.RemoveButton.OnClick.AddListener(new UnityAction(entry.RemoveClick ?? ActionExtensions.Empty));
				instance.RemoveButton.interactable = entry.IsRemovable;
			}
		}

		#region Events
		public void OnDoneClick() { DoneClick(); }
		#endregion
	}

	public interface IShipSlotsView : ICanvasView
	{
		string Title { set; }
		string Description { set; }
		float MaxRations { set; }
		float MaxFuel { set; }
		float MaxSpeed { set; }
		float Speed { set; }
		float RationGeneration { set; }
		float FuelGeneration { set; }
		float RationGenerationCap { set; }
		float FuelGenerationCap { set; }
		float RationConsumption { set; }
		ShipSlotBlock[] SlotEntries { set; }
		ShipModuleBlock[] ModuleEntries { set; }
		Action DoneClick { set; }
	}
}