using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class InventoryModel : Model
	{
		[JsonProperty] string inventoryId;
		[JsonProperty] string instanceId;
		[JsonProperty] string name;
		[JsonProperty] string description;
		[JsonProperty] string slotId;

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
		/// Gets a value indicating whether this <see cref="T:LunraGames.SpaceFarm.Models.InventoryModel"/> is slotted.
		/// </summary>
		/// <value><c>true</c> if is slotted; otherwise, <c>false</c>.</value>
		[JsonIgnore]
		public virtual bool IsUsable { get { return !SlotRequired || !string.IsNullOrEmpty(SlotId); } }

		[JsonIgnore]
		public readonly ListenerProperty<string> InventoryId;
		[JsonIgnore]
		public readonly ListenerProperty<string> InstanceId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Description;
		/// <summary>
		/// The slot identifier, this is the SlotId of a ModuleSlotModel that
		/// this model is slotted into. The ModuleSlotModel.ItemId should be
		/// this item's InstanceId.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> SlotId;

		public InventoryModel()
		{
			InventoryId = new ListenerProperty<string>(value => inventoryId = value, () => inventoryId);
			InstanceId = new ListenerProperty<string>(value => instanceId = value, () => instanceId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			SlotId = new ListenerProperty<string>(value => slotId = value, () => slotId);
		}
	}
}