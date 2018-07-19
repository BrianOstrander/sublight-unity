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
		[JsonProperty] OrbitalCrewInventoryModel[] orbitalCrews = new OrbitalCrewInventoryModel[0];
		[JsonProperty] ModuleInventoryModel[] modules = new ModuleInventoryModel[0];
		[JsonProperty] ResourceInventoryModel refillLogisticsResources = new ResourceInventoryModel();
		[JsonProperty] ResourceInventoryModel maximumLogisticsResources = new ResourceInventoryModel();
		[JsonProperty] ResourceInventoryModel maximumResources = new ResourceInventoryModel();
		[JsonProperty] ResourceInventoryModel maximumRefillableLogisticsResources = new ResourceInventoryModel();
		[JsonProperty] ResourceInventoryModel allResources = new ResourceInventoryModel();
		[JsonProperty] ResourceInventoryModel usableResources = new ResourceInventoryModel();
		[JsonProperty] ResourceInventoryModel unUsableResources = new ResourceInventoryModel();

		[JsonProperty] SlotEdge[] slotEdges = new SlotEdge[0];

		[JsonIgnore]
		public readonly ListenerProperty<SlotEdge[]> SlotEdges;
		#endregion

		#region Derived Values
		[JsonIgnore]
		public readonly ListenerProperty<InventoryModel[]> All;
		#endregion

		#region Shortcuts
		[JsonIgnore]
		public ResourceInventoryModel RefillLogisticsResources { get { return refillLogisticsResources; } }
		/// <summary>
		/// Gets the logistics resources, the total resources never goes below
		/// this amount.
		/// </summary>
		/// <value>The logistics resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel MaximumLogisticsResources { get { return maximumLogisticsResources; } }
		/// <summary>
		/// Gets the maximum resources this inventory list can store. Assigning
		/// values to this won't do anything, so don't do that...
		/// </summary>
		/// <value>The maximum resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel MaximumResources { get { return maximumResources; } }
		/// <summary>
		/// Gets the maximum refillable logistics resources, basically
		/// MaximumLogisticsResources clamped by MaximumLogisticsResources.
		/// </summary>
		/// <value>The maximum refillable logistics resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel MaximumRefillableLogisticsResources { get { return maximumRefillableLogisticsResources; } }
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
		public ResourceInventoryModel UsableResources { get { return usableResources; } }
		/// <summary>
		/// The total unusable resources contained by this list. Assigning
		/// values to this won't do anything, so don't do that...
		/// </summary>
		/// <value>The unusable resources.</value>
		[JsonIgnore]
		public ResourceInventoryModel UnUsableResources { get { return unUsableResources; } }
		#endregion

		public InventoryListModel()
		{
			// Derived Values
			All = new ListenerProperty<InventoryModel[]>(OnSetInventory, OnGetInventory);
			SlotEdges = new ListenerProperty<SlotEdge[]>(value => slotEdges = value, () => slotEdges, OnSlotEdges);

			AllResources.AnyChange += OnResources;
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
		/// required to have one. A resource item is only included if it's not
		/// zero.
		/// </summary>
		/// <returns>The unusable inventory.</returns>
		/// <param name="predicate">Predicate.</param>
		public InventoryModel[] GetUnUsableInventory(Func<InventoryModel, bool> predicate = null)
		{
			return GetUnUsableInventory<InventoryModel>(predicate);
		}

		/// <summary>
		/// Gets the unusable inventory, entries that are not slotted but are
		/// required to have one. A resource item is only included if it's not
		/// zero.
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

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:LunraGames.SpaceFarm.Models.InventoryListModel"/> has unused items or resources.
		/// </summary>
		/// <value><c>true</c> if has unused; otherwise, <c>false</c>.</value>
		[JsonIgnore]
		public bool HasUnused
		{
			get
			{
				if (!UnUsableResources.IsZero) return true;
				return 0 < GetUnUsableInventory().Length;
			}
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
			MaximumResources.Assign(newMaxResources);
			OnResources(AllResources);
		}

		void OnResources(ResourceInventoryModel model)
		{
			UnUsableResources.Assign(AllResources.ClampOut(MaximumResources, UsableResources));
		}

		void OnSetInventory(InventoryModel[] newInventory)
		{
			var hasAssignedResources = false;

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

			orbitalCrews = orbitalCrewList.ToArray();
			modules = moduleList.ToArray();

			SlotEdges.Value = slotEdgeList.ToArray();

			MaximumResources.Assign(newMaxResources);
			if (newResources != null) AllResources.Assign(newResources);
			else OnResources(AllResources);
		}

		InventoryModel[] OnGetInventory()
		{
			return orbitalCrews.Cast<InventoryModel>().Concat(modules)
													  .Append(allResources)
													  .ToArray();
		}
		#endregion
	}
}