using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class InventoryListModel : Model
	{
		#region Assigned Values
		[JsonProperty] OrbitalCrewInventoryModel[] orbitalCrews = new OrbitalCrewInventoryModel[0];
		[JsonProperty] ModuleInventoryModel[] modules = new ModuleInventoryModel[0];
		[JsonProperty] ResourceInventoryModel refillResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel refillLogisticsResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel maximumLogisticsResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel maximumResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel maximumRefillableLogisticsResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel allResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel usableResources = ResourceInventoryModel.Zero;
		[JsonProperty] ResourceInventoryModel unUsableResources = ResourceInventoryModel.Zero;

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
		/// The amount of resources this module should create or consume when 
		/// active, always.
		/// </summary>
		/// <value>Resources created or consumed per day.</value>
		[JsonIgnore]
		public ResourceInventoryModel RefillResources { get { return refillResources; } }
		/// <summary>
		/// The amount of resources this ship should created when active, per
		/// day. Should not be negative.
		/// </summary>
		/// <value>Resources created per day.</value>
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

		/// <summary>
		/// Don't know why I need this but I do, without this, this very event
		/// will not get binded, or it kind of will, but in the wrong context
		/// and never get called? I dunno, very weird, don't think about it too
		/// much.
		/// </summary>
		[OnDeserialized]
		void OnInitialized(StreamingContext context)
		{
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
				if (!UnUsableResources.IsZero) result = result.Append(UnUsableResources as T).ToArray();
			}
			if (predicate != null) result = result.Where(predicate).ToArray();
			return result;

		}

		/// <summary>
		/// Connect the specified slot and item. Will disconnect any other
		/// connections they might already have.
		/// </summary>
		/// <returns>The connection.</returns>
		/// <param name="parentSlotId">Slot identifier.</param>
		/// <param name="itemInstanceId">Item instance identifier.</param>
		public SlotEdge Connect(string parentSlotId, string itemInstanceId)
		{
			if (string.IsNullOrEmpty(parentSlotId)) throw new ArgumentNullException("slotId");
			if (string.IsNullOrEmpty(itemInstanceId)) throw new ArgumentNullException("itemInstanceId");

			var slot = GetInventory<ModuleInventoryModel>().SelectMany(m => m.Slots.All.Value).First(s => s.ParentSlotId.Value == parentSlotId);
			var item = GetInventoryFirstOrDefault(itemInstanceId);
			if (item == null) throw new ArgumentException("No item found with id: "+itemInstanceId, "itemInstanceId");
			return Connect(slot, item);
		}

		/// <summary>
		/// Connect the specified slot and item. Will disconnect any other
		/// connections they might already have.
		/// </summary>
		/// <returns>The connection.</returns>
		/// <param name="slot">Slot.</param>
		/// <param name="item">Item.</param>
		public SlotEdge Connect(ModuleSlotModel slot, InventoryModel item)
		{
			if (slot == null) throw new ArgumentNullException("slot");
			if (item == null) throw new ArgumentNullException("item");
			if (!slot.IsFillable) throw new ArgumentException("Specified slot cannot be filled.", "slot");
			if (!slot.CanSlot(item.InventoryType)) throw new ArgumentException("Specified slot cannot be filled with item of InventoryType: " + item.InventoryType, "slot");

			var parentSlotId = slot.ParentSlotId.Value;
			var itemId = item.InstanceId.Value;

			var existing = SlotEdges.Value.FirstOrDefault(e => e.ParentSlotId == parentSlotId && e.ItemInstanceId == itemId);
			if (!existing.IsEmpty) return existing;

			var result = new SlotEdge(parentSlotId, itemId);

			var remaining = RemoveConnection(parentSlotId, itemId);

			slot.ItemId.Value = result.ItemInstanceId;
			item.ParentSlotId.Value = result.ParentSlotId;
			SlotEdges.Value = remaining.Append(result).ToArray();
			return result;
		}

		/// <summary>
		/// Disconnects the specified connection.
		/// </summary>
		/// <returns>The disconnect.</returns>
		/// <param name="edge">Edge.</param>
		public void Disconnect(SlotEdge edge)
		{
			Disconnect(edge.ParentSlotId, edge.ItemInstanceId);
		}

		/// <summary>
		/// Disconnects any items from this slot, if there is a connection at
		/// all.
		/// </summary>
		/// <returns>The disconnect.</returns>
		/// <param name="slot">Slot.</param>
		public void Disconnect(ModuleSlotModel slot)
		{
			Disconnect(slot.ParentSlotId.Value, slot.ItemId.Value);
		}

		/// <summary>
		/// Disconnects this item from any slots, if there's any connection at
		/// all.
		/// </summary>
		/// <returns>The disconnect.</returns>
		/// <param name="item">Item.</param>
		public void Disconnect(InventoryModel item)
		{
			Disconnect(item.ParentSlotId.Value, item.InstanceId.Value);
		}

		/// <summary>
		/// Disconnects any connection between the specified slot and item, if
		/// there is any at all. If they're connected to anything else, those
		/// connections are preserved.
		/// </summary>
		/// <returns>The disconnect.</returns>
		/// <param name="parentSlotId">Slot identifier.</param>
		/// <param name="itemInstanceId">Item instance identifier.</param>
		public void Disconnect(string parentSlotId, string itemInstanceId)
		{
			if (string.IsNullOrEmpty(parentSlotId)) throw new ArgumentNullException("slotId");
			if (string.IsNullOrEmpty(itemInstanceId)) throw new ArgumentNullException("itemInstanceId");

			var existing = SlotEdges.Value.FirstOrDefault(e => e.ParentSlotId == parentSlotId && e.ItemInstanceId == itemInstanceId);
			if (existing.IsEmpty) return;

			SlotEdges.Value = RemoveConnection(parentSlotId, itemInstanceId).ToArray();
		}

		/// <summary>
		/// Helper method to remove all references of a slot and item without
		/// setting the list's SlotEdges. The specified slot and item don't
		/// neccesarily need to already be connected to each other, it just
		/// makes them available to be connected to each other.
		/// </summary>
		/// <returns>The remaining edges.</returns>
		/// <param name="parentSlotId">Slot identifier.</param>
		/// <param name="itemInstanceId">Item identifier.</param>
		IEnumerable<SlotEdge> RemoveConnection(string parentSlotId, string itemInstanceId)
		{
			foreach (var itemWithSlot in All.Value)
			{
				if (itemWithSlot.ParentSlotId.Value == parentSlotId) itemWithSlot.ParentSlotId.Value = null;
				if (itemWithSlot.InventoryType == InventoryTypes.Module)
				{
					var module = itemWithSlot as ModuleInventoryModel;
					foreach (var currSlot in module.Slots.All.Value.Where(s => s.ItemId.Value == itemInstanceId))
					{
						currSlot.ItemId.Value = null;
					}
				}
			}
			return SlotEdges.Value.Where(e => e.ParentSlotId != parentSlotId && e.ItemInstanceId != itemInstanceId);
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

		public void Add(InventoryModel entry)
		{
			All.Value = All.Value.Append(entry).ToArray();
		}
		#endregion

		#region Events
		void OnSlotEdges(SlotEdge[] edges)
		{
			var newRefillResources = ResourceInventoryModel.Zero;
			var newRefillLogicResources = ResourceInventoryModel.Zero;
			var newMaxLogicResources = ResourceInventoryModel.Zero;
			var newMaxResources = ResourceInventoryModel.Zero;

			foreach (var module in modules)
			{
				if (module.IsUsable)
				{
					newRefillResources.Add(module.Slots.RefillResources);
					newRefillLogicResources.Add(module.Slots.RefillLogisticsResources);
					newMaxLogicResources.Add(module.Slots.MaximumLogisticsResources);
					newMaxResources.Add(module.Slots.MaximumResources);
				}
			}

			RefillResources.Assign(newRefillResources);
			RefillLogisticsResources.Assign(newRefillLogicResources.ClampNegatives());
			MaximumLogisticsResources.Assign(newMaxLogicResources);
			MaximumResources.Assign(newMaxResources);
			MaximumRefillableLogisticsResources.Assign(MaximumLogisticsResources.Duplicate.Clamp(MaximumResources));

			OnResources(AllResources);
		}

		void OnResources(ResourceInventoryModel model)
		{
			ResourceInventoryModel newUnUsableResources;
			UsableResources.Assign(AllResources.Duplicate.Clamp(MaximumResources, out newUnUsableResources));
			UnUsableResources.Assign(newUnUsableResources);
		}

		void OnSetInventory(InventoryModel[] newInventory)
		{
			var hasAssignedResources = false;

			var orbitalCrewList = new List<OrbitalCrewInventoryModel>();
			var moduleList = new List<ModuleInventoryModel>();
			ResourceInventoryModel newResources = null;
			var slotEdgeList = new List<SlotEdge>();

			var newRefillResources = ResourceInventoryModel.Zero;
			var newRefillLogicResources = ResourceInventoryModel.Zero;
			var newMaxLogicResources = ResourceInventoryModel.Zero;
			var newMaxResources = ResourceInventoryModel.Zero;

			foreach (var inventory in newInventory)
			{
				if (!string.IsNullOrEmpty(inventory.ParentSlotId.Value)) slotEdgeList.Add(new SlotEdge(inventory.ParentSlotId, inventory.InstanceId));
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
						if (module.IsUsable)
						{
							newRefillResources.Add(module.Slots.RefillResources);
							newRefillLogicResources.Add(module.Slots.RefillLogisticsResources);
							newMaxLogicResources.Add(module.Slots.MaximumLogisticsResources);
							newMaxResources.Add(module.Slots.MaximumResources);
						}
						break;
					default:
						Debug.LogError("Unrecognized InventoryType: " + inventory.InventoryType);
						break;
				}
			}

			orbitalCrews = orbitalCrewList.ToArray();
			modules = moduleList.ToArray();

			SlotEdges.Value = slotEdgeList.ToArray();

			RefillResources.Assign(newRefillResources);
			RefillLogisticsResources.Assign(newRefillLogicResources.ClampNegatives());
			MaximumLogisticsResources.Assign(newMaxLogicResources);
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