﻿using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class InventoryModel : Model
	{
		[JsonProperty] bool hidden;
		[JsonProperty] string inventoryId;
		[JsonProperty] string instanceId;
		[JsonProperty] string name;
		[JsonProperty] string description;
		[JsonProperty] string parentSlotId;

		/// <summary>
		/// Gets the type of the inventory item.
		/// </summary>
		/// <value>The type of the inventory item.</value>
		[JsonIgnore]
		public abstract InventoryTypes InventoryType { get; }
		/// <summary>
		/// Can you slot this in a module?
		/// </summary>
		/// <value><c>true</c> if can slot; otherwise, <c>false</c>.</value>
		[JsonIgnore]
		public virtual bool SlotRequired { get { return true; } }
		/// <summary>
		/// Gets a value indicating whether this <see cref="T:LunraGames.SpaceFarm.Models.InventoryModel"/> is slotted or needs to be.
		/// </summary>
		/// <value><c>true</c> if is slotted; otherwise, <c>false</c>.</value>
		[JsonIgnore]
		public virtual bool IsUsable { get { return !SlotRequired || !string.IsNullOrEmpty(ParentSlotId); } }
		/// <summary>
		/// Gets a value indicating whether this <see cref="T:LunraGames.SpaceFarm.Models.InventoryModel"/> is slotted in the inventory.
		/// </summary>
		/// <value><c>true</c> if is slotted; otherwise, <c>false</c>.</value>
		[JsonIgnore]
		public bool IsSlotted { get { return !string.IsNullOrEmpty(ParentSlotId.Value); } }

		[JsonIgnore]
		public readonly ListenerProperty<bool> Hidden;
		[JsonIgnore]
		public readonly ListenerProperty<string> InventoryId;
		[JsonIgnore]
		public readonly ListenerProperty<string> InstanceId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Description;
		/// <summary>
		/// The slot identifier, this is the ParentSlotId of a ModuleSlotModel
		/// that this model is slotted into. The ModuleSlotModel.ItemId should
		/// be this item's InstanceId.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> ParentSlotId;

		public InventoryModel()
		{
			Hidden = new ListenerProperty<bool>(value => hidden = value, () => hidden);
			InventoryId = new ListenerProperty<string>(value => inventoryId = value, () => inventoryId);
			InstanceId = new ListenerProperty<string>(value => instanceId = value, () => instanceId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			ParentSlotId = new ListenerProperty<string>(value => parentSlotId = value, () => parentSlotId);
		}
	}
}