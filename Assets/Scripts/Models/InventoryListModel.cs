using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class InventoryListModel : Model
	{
		#region Assigned Values
		[JsonProperty] OrbitalProbeInventoryModel[] orbitalProbes = new OrbitalProbeInventoryModel[0];
		[JsonProperty] OrbitalCrewInventoryModel[] orbitalCrews = new OrbitalCrewInventoryModel[0];
		[JsonProperty] ModuleInventoryModel[] modules = new ModuleInventoryModel[0];
		[JsonProperty] ResourceInventoryModel maximumResources = new ResourceInventoryModel();
		[JsonProperty] ResourceInventoryModel allResources = new ResourceInventoryModel();
		[JsonProperty] ResourceInventoryModel usableresources = new ResourceInventoryModel();
		[JsonProperty] ResourceInventoryModel unUsableresources = new ResourceInventoryModel();

		[JsonProperty] SlotEdge[] slotEdges = new SlotEdge[0];

		[JsonIgnore]
		public readonly ListenerProperty<SlotEdge[]> SlotEdges;
		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<InventoryModel[]> All;
		#endregion

		#region Shortcuts
		/// <summary>
		/// Gets the maximum resources this inventory list can store. Assigning
		/// values to this won't do anything, so don't do that...
		/// </summary>
		/// <value>The maximum resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel MaximumResources { get { return maximumResources; } }
		/// <summary>
		/// The total resources contained by this inventory list, usable and
		/// unusable.
		/// </summary>
		/// <value>The resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel AllResources { get { return allResources; } }
		/// <summary>
		/// The total usable resources contained by this list. Assigning values
		/// to this won't do anything, so don't do that...
		/// </summary>
		/// <value>The usable resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel UsableResources { get { return usableresources; } }
		/// <summary>
		/// The total unusable resources contained by this list. Assigning
		/// values to this won't do anything, so don't do that...
		/// </summary>
		/// <value>The unusable resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel UnUsableResources { get { return unUsableresources; } }
		#endregion

		public InventoryListModel()
		{
			// Derived Values
			All = new ListenerProperty<InventoryModel[]>(OnSetInventory, OnGetInventory);
			SlotEdges = new ListenerProperty<SlotEdge[]>(value => slotEdges = value, () => slotEdges, OnSlotEdges);

			allResources.AnyChange += OnResources;
		}

		#region Utility
		public InventoryModel[] GetInventory(Func<InventoryModel, bool> predicate = null)
		{
			return GetInventory<InventoryModel>(predicate);
		}

		public T[] GetInventory<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			if (predicate == null) return All.Value.OfType<T>().ToArray();
			return All.Value.OfType<T>().Where(predicate).ToArray();
		}

		public InventoryModel GetInventoryFirstOrDefault(string instanceId)
		{
			return GetInventoryFirstOrDefault<InventoryModel>(instanceId);
		}

		public T GetInventoryFirstOrDefault<T>(string instanceId) where T : InventoryModel
		{
			return GetInventoryFirstOrDefault<T>(i => i.InstanceId == instanceId);
		}

		public InventoryModel GetInventoryFirstOrDefault(Func<InventoryModel, bool> predicate = null)
		{
			return GetInventoryFirstOrDefault<InventoryModel>(predicate);
		}

		public T GetInventoryFirstOrDefault<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			if (predicate == null) return All.Value.OfType<T>().FirstOrDefault();
			return All.Value.OfType<T>().FirstOrDefault(predicate);
		}

		public InventoryTypes[] GetTypes()
		{
			return All.Value.Select(i => i.InventoryType).Distinct().ToArray();
		}

		/// <summary>
		/// Gets the usable inventory, entries that are slotted or don't need
		/// to be slotted.
		/// </summary>
		/// <returns>The usable inventory.</returns>
		/// <param name="predicate">Predicate.</param>
		public InventoryModel[] GetUsableInventory(Func<InventoryModel, bool> predicate = null)
		{
			return GetUsableInventory<InventoryModel>(predicate);
		}

		/// <summary>
		/// Gets the usable inventory, entries that are slotted or don't need
		/// to be slotted.
		/// </summary>
		/// <returns>The usable inventory.</returns>
		/// <param name="predicate">Predicate.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T[] GetUsableInventory<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			var result = GetInventory<T>(i => i.InventoryType != InventoryTypes.Resources && i.IsUsable);
			if (typeof(T) == typeof(InventoryModel) || typeof(T) == typeof(ResourceInventoryModel))
			{
				result = result.Append(UsableResources as T).ToArray();
			}
			if (predicate != null) result = result.Where(predicate).ToArray();
			return result;
		}

		/// <summary>
		/// Gets the unusable inventory, entries that are not slotted but are
		/// required to have one.
		/// </summary>
		/// <returns>The unusable inventory.</returns>
		/// <param name="predicate">Predicate.</param>
		public InventoryModel[] GetUnUsableInventory(Func<InventoryModel, bool> predicate = null)
		{
			return GetUnUsableInventory<InventoryModel>(predicate);
		}

		/// <summary>
		/// Gets the unusable inventory, entries that are not slotted but are
		/// required to have one.
		/// </summary>
		/// <returns>The unusable inventory.</returns>
		/// <param name="predicate">Predicate.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T[] GetUnUsableInventory<T>(Func<T, bool> predicate = null) where T : InventoryModel
		{
			var result = GetInventory<T>(i => i.InventoryType != InventoryTypes.Resources && !i.IsUsable);
			if (typeof(T) == typeof(InventoryModel) || typeof(T) == typeof(ResourceInventoryModel))
			{
				var unUsableResources = UnUsableResources;
				if (!unUsableResources.IsZero) result = result.Append(unUsableResources as T).ToArray();
			}
			if (predicate != null) result = result.Where(predicate).ToArray();
			return result;

		}

		public SlotEdge Connect(string slotId, string itemInstanceId)
		{
			if (string.IsNullOrEmpty(slotId)) throw new ArgumentNullException("slotId");
			if (string.IsNullOrEmpty(itemInstanceId)) throw new ArgumentNullException("itemInstanceId");

			var slot = GetInventory<ModuleInventoryModel>().SelectMany(m => m.Slots.All.Value).First(s => s.SlotId.Value == slotId);
			var item = GetInventoryFirstOrDefault(itemInstanceId);
			if (item == null) throw new ArgumentException("No item found with id: "+itemInstanceId, "itemInstanceId");
			return Connect(slot, item);
		}

		public SlotEdge Connect(ModuleSlotModel slot, InventoryModel item)
		{
			if (slot == null) throw new ArgumentNullException("slot");
			if (item == null) throw new ArgumentNullException("item");

			var slotId = slot.SlotId.Value;
			var itemId = item.InstanceId.Value;

			var existing = SlotEdges.Value.FirstOrDefault(e => e.SlotId == slotId && e.ItemInstanceId == itemId);
			if (!existing.IsEmpty) return existing;

			var result = new SlotEdge(slotId, itemId);

			var remaining = RemoveConnection(slotId, itemId);

			slot.ItemId.Value = result.ItemInstanceId;
			item.SlotId.Value = result.SlotId;
			SlotEdges.Value = remaining.Append(result).ToArray();
			return result;
		}

		public void Disconnect(SlotEdge edge)
		{
			Disconnect(edge.SlotId, edge.ItemInstanceId);
		}

		public void Disconnect(ModuleSlotModel slot)
		{
			Disconnect(slot.SlotId.Value, slot.ItemId.Value);
		}

		public void Disconnect(InventoryModel item)
		{
			Disconnect(item.SlotId.Value, item.InstanceId.Value);
		}

		public void Disconnect(string slotId, string itemInstanceId)
		{
			if (string.IsNullOrEmpty(slotId)) throw new ArgumentNullException("slotId");
			if (string.IsNullOrEmpty(itemInstanceId)) throw new ArgumentNullException("itemInstanceId");

			var existing = SlotEdges.Value.FirstOrDefault(e => e.SlotId == slotId && e.ItemInstanceId == itemInstanceId);
			if (existing.IsEmpty) return;

			SlotEdges.Value = RemoveConnection(slotId, itemInstanceId).ToArray();
		}

		/// <summary>
		/// Helper method to remove all references of a slot and item without
		/// setting the list's SlotEdges.
		/// </summary>
		/// <returns>The remaining edges.</returns>
		/// <param name="slotId">Slot identifier.</param>
		/// <param name="itemInstanceId">Item identifier.</param>
		IEnumerable<SlotEdge> RemoveConnection(string slotId, string itemInstanceId)
		{
			foreach (var itemWithSlot in All.Value)
			{
				if (itemWithSlot.SlotId.Value == slotId) itemWithSlot.SlotId.Value = null;
				if (itemWithSlot.InventoryType == InventoryTypes.Module)
				{
					var module = itemWithSlot as ModuleInventoryModel;
					foreach (var currSlot in module.Slots.All.Value.Where(s => s.ItemId.Value == itemInstanceId))
					{
						currSlot.ItemId.Value = null;
					}
				}
			}
			foreach (var slotWithItem in GetInventory(i => i.InventoryType == InventoryTypes.Module).Cast<ModuleInventoryModel>().SelectMany(m => m.Slots.All.Value).Where(s => s.ItemId.Value == itemInstanceId))
			{
				slotWithItem.SlotId.Value = null;
				slotWithItem.ItemId.Value = null;
			}
			return SlotEdges.Value.Where(e => e.SlotId != slotId && e.ItemInstanceId != itemInstanceId);
		}

		public void ClearUnused()
		{
			var unUsedResources = UnUsableResources.Duplicate;
			var toRemove = GetUnUsableInventory(i => i.InventoryType != InventoryTypes.Resources);
			var removedIds = new List<string>();
			foreach (var unused in toRemove)
			{
				removedIds.Add(unused.InstanceId);
				if (unused.IsSlotted) Disconnect(unused);
			}
			All.Value = All.Value.Where(i => !removedIds.Contains(i.InstanceId)).ToArray();
			if (!unUsedResources.IsZero) AllResources.Subtract(unUsedResources);
		}
		#endregion

		#region Events
		void OnSlotEdges(SlotEdge[] edges)
		{
			var newMaxResources = ResourceInventoryModel.Zero;
			foreach (var module in modules)
			{
				if (module.IsUsable) newMaxResources.Add(module.Slots.MaximumResources);
			}
			maximumResources.Assign(newMaxResources);
			OnResources(allResources);
		}

		void OnResources(ResourceInventoryModel model)
		{
			unUsableresources.Assign(allResources.ClampOut(maximumResources, usableresources));
		}

		void OnSetInventory(InventoryModel[] newInventory)
		{
			var hasAssignedResources = false;

			var orbitalProbeList = new List<OrbitalProbeInventoryModel>();
			var orbitalCrewList = new List<OrbitalCrewInventoryModel>();
			var moduleList = new List<ModuleInventoryModel>();
			var newMaxResources = ResourceInventoryModel.Zero;
			ResourceInventoryModel newResources = null;
			var slotEdgeList = new List<SlotEdge>();

			foreach (var inventory in newInventory)
			{
				if (!string.IsNullOrEmpty(inventory.SlotId.Value)) slotEdgeList.Add(new SlotEdge(inventory.SlotId, inventory.InstanceId));
				switch (inventory.InventoryType)
				{
					case InventoryTypes.OrbitalProbe:
						orbitalProbeList.Add(inventory as OrbitalProbeInventoryModel);
						break;
					case InventoryTypes.OrbitalCrew:
						orbitalCrewList.Add(inventory as OrbitalCrewInventoryModel);
						break;
					case InventoryTypes.Resources:
						if (hasAssignedResources) 
						{
							Debug.LogError("Multiple resource models passed to OnSetInventory, ignoring additional ones.");
							break;
						}
						hasAssignedResources = true;
						newResources = inventory as ResourceInventoryModel;
						break;
					case InventoryTypes.Module:
						var module = inventory as ModuleInventoryModel;
						moduleList.Add(module);
						if (module.IsUsable) newMaxResources.Add(module.Slots.MaximumResources);
						break;
					default:
						Debug.LogError("Unrecognized InventoryType: " + inventory.InventoryType);
						break;
				}
			}

			orbitalProbes = orbitalProbeList.ToArray();
			orbitalCrews = orbitalCrewList.ToArray();
			modules = moduleList.ToArray();

			SlotEdges.Value = slotEdgeList.ToArray();

			maximumResources.Assign(newMaxResources);
			if (newResources != null) allResources.Assign(newResources);
			else OnResources(allResources);
		}

		InventoryModel[] OnGetInventory()
		{
			return orbitalProbes.Cast<InventoryModel>().Concat(orbitalCrews)
													   .Concat(modules)
													   .Append(allResources)
													   .ToArray();
		}
		#endregion
	}
}