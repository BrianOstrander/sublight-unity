using System;
using System.Linq;

using Newtonsoft.Json;

using UnityEngine;

using LunraGames.SpaceFarm.Models;

using System.Collections.Generic;

namespace LunraGames.SpaceFarm
{
	/// <summary>
	/// A connection between a slot and an item.
	/// </summary>
	[Serializable]
	public struct SlotEdge
	{
		public static SlotEdge Connect(string slotId, string itemId, InventoryListModel list)
		{
			var slot = list.GetInventory<ModuleInventoryModel>().SelectMany(m => m.Slots.All.Value).First(s => s.SlotId.Value == slotId);
			var item = list.GetInventoryFirstOrDefault(itemId);
			if (item == null) throw new ArgumentException("No item found with that id", "itemId");
			return Connect(slot, item, list);
		}

		public static SlotEdge Connect(ModuleSlotModel slot, InventoryModel item, InventoryListModel list)
		{
			if (slot == null) throw new ArgumentNullException("slot");
			if (item == null) throw new ArgumentNullException("item");
			if (list == null) throw new ArgumentNullException("list");

			var slotId = slot.SlotId.Value;
			var itemId = item.InstanceId.Value;

			var existing = list.SlotEdges.Value.FirstOrDefault(e => e.SlotId == slotId && e.ItemId == itemId);
			if (!existing.IsEmpty) return existing;

			var result = new SlotEdge(slotId, itemId);

			var remaining = Remove(slotId, itemId, list);

			slot.ItemId.Value = result.ItemId;
			item.SlotId.Value = result.SlotId;
			list.SlotEdges.Value = remaining.Append(result).ToArray();
			return result;
		}

		public static void Disconnect(SlotEdge edge, InventoryListModel list)
		{
			Disconnect(edge.SlotId, edge.ItemId, list);
		}

		public static void Disconnect(ModuleSlotModel slot, InventoryListModel list)
		{
			Disconnect(slot.SlotId.Value, slot.ItemId.Value, list);
		}

		public static void Disconnect(InventoryModel item, InventoryListModel list)
		{
			Disconnect(item.SlotId.Value, item.InstanceId.Value, list);
		}

		public static void Disconnect(string slotId, string itemId, InventoryListModel list)
		{
			var existing = list.SlotEdges.Value.FirstOrDefault(e => e.SlotId == slotId && e.ItemId == itemId);
			if (existing.IsEmpty) return;

			list.SlotEdges.Value = Remove(slotId, itemId, list).ToArray();
		}

		/// <summary>
		/// Helper method to remove all references of a slot and item without
		/// setting the list's SlotEdges.
		/// </summary>
		/// <returns>The remaining edges.</returns>
		/// <param name="slotId">Slot identifier.</param>
		/// <param name="itemId">Item identifier.</param>
		/// <param name="list">List.</param>
		static IEnumerable<SlotEdge> Remove(string slotId, string itemId, InventoryListModel list)
		{
			foreach (var itemWithSlot in list.All.Value)
			{
				if (itemWithSlot.SlotId.Value == slotId) itemWithSlot.SlotId.Value = null;
				if (itemWithSlot.InventoryType == InventoryTypes.Module)
				{
					var module = itemWithSlot as ModuleInventoryModel;
					foreach (var currSlot in module.Slots.All.Value.Where(s => s.ItemId.Value == itemId))
					{
						currSlot.ItemId.Value = null;
					}
				}
			}
			foreach (var slotWithItem in list.GetInventory(i => i.InventoryType == InventoryTypes.Module).Cast<ModuleInventoryModel>().SelectMany(m => m.Slots.All.Value).Where(s => s.ItemId.Value == itemId))
			{
				slotWithItem.SlotId.Value = null;
				slotWithItem.ItemId.Value = null;
			}
			return list.SlotEdges.Value.Where(e => e.SlotId != slotId && e.ItemId != itemId);
		}

		[JsonProperty] public readonly string SlotId;
		[JsonProperty] public readonly string ItemId;

		[JsonIgnore]
		public bool IsEmpty { get { return string.IsNullOrEmpty(SlotId) || string.IsNullOrEmpty(ItemId); } }

		public SlotEdge(string slotId, string itemId)
		{
			SlotId = slotId;
			ItemId = itemId;
		}

		public override string ToString()
		{
			return "SlotEdge: SlotId: " + SlotId + " ItemId: " + ItemId;
		}
	}
}