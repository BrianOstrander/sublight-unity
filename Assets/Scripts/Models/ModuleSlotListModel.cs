using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ModuleSlotListModel : Model
	{
		#region Assigned Values
		[JsonProperty] CrewModuleSlotModel[] crews = new CrewModuleSlotModel[0];
		[JsonProperty] ResourceModuleSlotModel[] resources = new ResourceModuleSlotModel[0];
		[JsonProperty] ModuleModuleSlotModel[] modules = new ModuleModuleSlotModel[0];
		[JsonProperty] ResourceInventoryModel refillResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel refillLogisticsResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel maximumLogisticsResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel maximumResources = ResourceInventoryModel.Zero;
		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<ModuleSlotModel[]> All;
		#endregion

		#region Shortcuts
		/// <summary>
		/// The amount of resources this module should create or consume when 
		/// active, always.
		/// </summary>
		/// <value>Resources created or consumed per day.</value>
		[JsonIgnore]
		public ResourceInventoryModel RefillResources { get { return refillResources; } }
		/// <summary>
		/// The amount of resources this module should create when active and
		/// logistics space is available. Should not be negative.
		/// </summary>
		/// <value>Resources created per day.</value>
		[JsonIgnore]
		public ResourceInventoryModel RefillLogisticsResources { get { return refillLogisticsResources; } }
		/// <summary>
		/// Gets amount this module increases the logistics maximum by.
		/// </summary>
		/// <value>The maximum logistics resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel MaximumLogisticsResources { get { return maximumLogisticsResources; } }
		/// <summary>
		/// The amount this module increases the maximum storable resources by.
		/// </summary>
		/// <value>The maximum resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel MaximumResources { get { return maximumResources; } }
		#endregion

		public ModuleSlotListModel()
		{
			// Derived Values
			All = new ListenerProperty<ModuleSlotModel[]>(OnSetSlots, OnGetSlots);
		}

		#region Utility
		public T[] GetSlots<T>(Func<T, bool> predicate = null) where T : ModuleSlotModel
		{
			if (predicate == null) return All.Value.OfType<T>().ToArray();
			return All.Value.OfType<T>().Where(predicate).ToArray();
		}

		public T GetSlotFirstOrDefault<T>(string slotId) where T : ModuleSlotModel
		{
			return GetSlotFirstOrDefault<T>(i => i.SlotId == slotId);
		}

		public T GetSlotFirstOrDefault<T>(Func<T, bool> predicate = null) where T : ModuleSlotModel
		{
			if (predicate == null) return All.Value.OfType<T>().FirstOrDefault();
			return All.Value.OfType<T>().FirstOrDefault(predicate);
		}

		public SlotTypes[] GetTypes()
		{
			return All.Value.Select(i => i.SlotType).Distinct().ToArray();
		}

		public T[] GetFullSlots<T>(Func<T, bool> predicate = null) where T : ModuleSlotModel
		{
			if (predicate == null) return GetSlots<T>(s => s.IsFillable && !s.IsFilled).ToArray();
			return GetSlots<T>(s => s.IsFillable && !s.IsFilled).Where(predicate).ToArray();
		}

		public T[] GetEmptySlots<T>(Func<T, bool> predicate = null) where T : ModuleSlotModel
		{
			if (predicate == null) return GetSlots<T>(s => s.IsFilled).ToArray();
			return GetSlots<T>(s => s.IsFilled).Where(predicate).ToArray();
		}
		#endregion

		#region Events
		void OnSetSlots(ModuleSlotModel[] newSlots)
		{
			var crewList = new List<CrewModuleSlotModel>();
			var resourceList = new List<ResourceModuleSlotModel>();
			var modulesList = new List<ModuleModuleSlotModel>();

			var newRefillResources = ResourceInventoryModel.Zero;
			var newRefillLogicResources = ResourceInventoryModel.Zero;
			var newMaxLogicResources = ResourceInventoryModel.Zero;
			var newMaxResources = ResourceInventoryModel.Zero;

			foreach (var slot in newSlots)
			{
				switch (slot.SlotType)
				{
					case SlotTypes.Crew:
						crewList.Add(slot as CrewModuleSlotModel);
						break;
					case SlotTypes.Resource:
						var resourceSlot = slot as ResourceModuleSlotModel;
						resourceList.Add(resourceSlot);
						newRefillResources.Add(resourceSlot.RefillResources);
						newRefillLogicResources.Add(resourceSlot.RefillLogisticsResources);
						newMaxLogicResources.Add(resourceSlot.MaximumLogisticsResources);
						newMaxResources.Add(resourceSlot.MaximumResources);
						break;
					case SlotTypes.Module:
						modulesList.Add(slot as ModuleModuleSlotModel);
						break;
					default:
						Debug.LogError("Unrecognized SlotType: " + slot.SlotType);
						break;
				}
			}

			crews = crewList.ToArray();
			resources = resourceList.ToArray();
			modules = modulesList.ToArray();

			RefillResources.Assign(newRefillResources);
			RefillLogisticsResources.Assign(newRefillLogicResources.Maximum(ResourceInventoryModel.Zero));
			MaximumLogisticsResources.Assign(newMaxLogicResources);
			MaximumResources.Assign(newMaxResources);
		}

		ModuleSlotModel[] OnGetSlots()
		{
			return crews.Cast<ModuleSlotModel>().Concat(resources)
												.Concat(modules)
												.ToArray();
		}
		#endregion
	}
}