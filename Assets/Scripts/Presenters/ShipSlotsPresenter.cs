using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ShipSlotsPresenter : Presenter<IShipSlotsView>
	{
		GameModel model;
		ShipModel ship;

		ModuleSlotModel selectedSlot;

		public ShipSlotsPresenter(GameModel model)
		{
			this.model = model;
			ship = model.Ship.Value;

			App.Callbacks.FocusRequest += OnFocus;

			ship.Inventory.MaximumResources.Rations.Changed += OnMaxRations;
			ship.Inventory.MaximumResources.Fuel.Changed += OnMaxFuel;
			ship.MaximumSpeed.Changed += OnMaxSpeed;
			ship.Speed.Changed += OnSpeed;
			ship.Inventory.UnUsableResources.Rations.Changed += OnUnusedRations;
			ship.Inventory.UnUsableResources.Fuel.Changed += OnUnusedFuel;
			ship.Inventory.RefillLogisticsResources.Rations.Changed += OnRationGeneration;
			ship.Inventory.RefillLogisticsResources.Fuel.Changed += OnFuelGeneration;
			ship.Inventory.MaximumRefillableLogisticsResources.Rations.Changed += OnRationGenerationCap;
			ship.Inventory.MaximumRefillableLogisticsResources.Fuel.Changed += OnFuelGenerationCap;
			ship.Inventory.RefillResources.Rations.Changed += OnRationConsumption;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.FocusRequest -= OnFocus;

			ship.Inventory.MaximumResources.Rations.Changed -= OnMaxRations;
			ship.Inventory.MaximumResources.Fuel.Changed -= OnMaxFuel;
			ship.MaximumSpeed.Changed -= OnMaxSpeed;
			ship.Speed.Changed -= OnSpeed;
			ship.Inventory.UnUsableResources.Rations.Changed -= OnUnusedRations;
			ship.Inventory.UnUsableResources.Fuel.Changed -= OnUnusedFuel;
			ship.Inventory.RefillLogisticsResources.Rations.Changed -= OnRationGeneration;
			ship.Inventory.RefillLogisticsResources.Fuel.Changed -= OnFuelGeneration;
			ship.Inventory.MaximumRefillableLogisticsResources.Rations.Changed -= OnRationGenerationCap;
			ship.Inventory.MaximumRefillableLogisticsResources.Fuel.Changed -= OnFuelGenerationCap;
			ship.Inventory.RefillResources.Rations.Changed -= OnRationConsumption;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = "Modify Ship Modules";
			View.Description = "Modules enhance your ship, change which one's you're using here.";

			View.DoneClick = OnDoneClick;

			View.MaxRations = ship.Inventory.MaximumResources.Rations;
			View.MaxFuel = ship.Inventory.MaximumResources.Fuel;
			View.MaxSpeed = ship.MaximumSpeed;
			View.Speed = ship.Speed;
			View.LooseRations = ship.Inventory.UnUsableResources.Rations;
			View.LooseFuel = ship.Inventory.UnUsableResources.Fuel;
			View.RationGeneration = ship.Inventory.RefillLogisticsResources.Rations;
			View.FuelGeneration = ship.Inventory.RefillLogisticsResources.Fuel;
			View.RationGenerationCap = ship.Inventory.MaximumRefillableLogisticsResources.Rations;
			View.FuelGenerationCap = ship.Inventory.MaximumRefillableLogisticsResources.Fuel;
			View.RationConsumption = ship.Inventory.RefillResources.Rations;

			selectedSlot = null;
			OnUpdateSlotsAndModules();

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Ship:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var shipFocus = focus as ShipFocusRequest;
					// We also only show up if our view is specified
					if (shipFocus.View != ShipFocusRequest.Views.SlotEditor) goto default;
					Show();
					break;
				default:
					if (View.TransitionState == TransitionStates.Shown) CloseView();
					break;
			}
		}

		void OnUpdateSlotsAndModules()
		{
			OnUpdateSlots();
			OnUpdateModules();
		}

		void OnUpdateSlots()
		{
			var slots = new List<ShipSlotBlock>();
			foreach (var module in ship.Inventory.GetUsableInventory<ModuleInventoryModel>())
			{
				var slotIndex = 0;
				foreach (var slot in module.Slots.All.Value)
				{
					if (!slot.IsFillable) continue;

					var itemName = "Empty";
					InventoryModel item = null;
					if (slot.IsFilled)
					{
						item = ship.Inventory.GetInventoryFirstOrDefault(slot.ItemId.Value);
						itemName = string.IsNullOrEmpty(item.Name) ? "<No Name>" : item.Name;
					}
					var slotType = string.Empty;
					switch(slot.SlotType)
					{
						case SlotTypes.Crew: slotType = "Shuttle Berth"; break;
						case SlotTypes.Module: slotType = "Module Slot"; break;
					}

					var entry = new ShipSlotBlock
					{
						SlotName = module.Name.Value + " - " + slotIndex,
						TypeName = slotType,
						ItemName = itemName,
						IsSelected = selectedSlot != null && slot.SlotId.Value == selectedSlot.SlotId.Value,
						Click = () => OnSlotClick(module, slot, item)
					};
					slots.Add(entry);
					slotIndex++;
				}
			}
			View.SlotEntries = slots.ToArray();
		}

		void OnUpdateModules()
		{
			if (selectedSlot == null)
			{
				View.ModuleEntries = null;
				return;
			}

			var modules = new List<ShipModuleBlock>();
			foreach (var item in ship.Inventory.GetInventory(i => i.SlotRequired && selectedSlot.CanSlot(i.InventoryType)))
			{
				if (item.InventoryType == InventoryTypes.Module)
				{
					var itemModule = item as ModuleInventoryModel;
					if (itemModule.IsRoot.Value) continue;
				}
				var currentlySlotted = item.SlotId.Value == selectedSlot.SlotId.Value;
				var buttonText = "Assign";

				if (currentlySlotted) buttonText = "Already Assigned";
				else if (item.IsSlotted) buttonText = "Reassign";

				var entry = new ShipModuleBlock
				{
					Name = item.Name,
					Description = item.Description,
					ButtonText = buttonText,
					IsSlotted = item.IsSlotted,
					CurrentlySlotted = currentlySlotted,
					AssignClick = () => OnAssignModuleClick(item),
					IsRemovable = currentlySlotted,
					RemoveClick = () => OnRemoveModuleClick(item),
				};
				modules.Add(entry);
			}
			View.ModuleEntries = modules.ToArray();
		}

		void OnSlotClick(ModuleInventoryModel module, ModuleSlotModel slot, InventoryModel item)
		{
			selectedSlot = slot;
			OnUpdateSlotsAndModules();
		}

		void OnAssignModuleClick(InventoryModel item)
		{
			ship.Inventory.Connect(selectedSlot, item);
			OnUpdateSlotsAndModules();
		}

		void OnRemoveModuleClick(InventoryModel item)
		{
			ship.Inventory.Disconnect(item);
			OnUpdateSlotsAndModules();
		}

		void OnDoneClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			App.Callbacks.FocusRequest(
				new SystemsFocusRequest(
					ship.Position.Value.SystemZero,
					ship.Position.Value
				)
			);
		}

		void OnMaxRations(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.MaxRations = value;
		}

		void OnMaxFuel(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.MaxFuel = value;
		}

		void OnMaxSpeed(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.MaxSpeed = value;
		}

		void OnSpeed(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.Speed = value;
		}

		void OnUnusedRations(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.LooseRations = value;
		}

		void OnUnusedFuel(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.LooseFuel = value;
		}

		void OnRationGeneration(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.RationGeneration = value;
		}

		void OnFuelGeneration(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.FuelGeneration = value;
		}

		void OnRationGenerationCap(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.RationGenerationCap = value;
		}

		void OnFuelGenerationCap(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.FuelGenerationCap = value;
		}

		void OnRationConsumption(float value)
		{
			if (View.TransitionState != TransitionStates.Shown) return;
			View.RationConsumption = value;
		}
		#endregion

	}
}