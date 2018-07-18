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
		[JsonProperty] ResourceInventoryModel maximumResources = new ResourceInventoryModel();
		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<ModuleSlotModel[]> All;
		#endregion

		#region Shortcuts
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
						newMaxResources.Add(resourceSlot.MaximumResources);
						break;
					default:
						Debug.LogError("Unrecognized SlotType: " + slot.SlotType);
						break;
				}
			}

			crews = crewList.ToArray();
			resources = resourceList.ToArray();
			
			maximumResources.Assign(newMaxResources);
		}

		ModuleSlotModel[] OnGetSlots()
		{
			return crews.Cast<ModuleSlotModel>().Concat(resources).ToArray();
		}
		#endregion
	}
}