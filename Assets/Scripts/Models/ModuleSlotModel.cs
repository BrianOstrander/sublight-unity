using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class ModuleSlotModel : Model
	{
		[JsonProperty] int index;
		[JsonProperty] string slotId;
		[JsonProperty] string parentSlotId;
		[JsonProperty] string itemId;

		/// <summary>
		/// The order these appear in the editor, not used in game for anything
		/// meaningful.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<int> Index;
		/// <summary>
		/// The slot identifier, the base id for this slot that is not unique.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> SlotId;
		/// <summary>
		/// The parent slot identifier, inventory items should specify this in
		/// their ParentSlotId to slot themselves here. This is unique.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> ParentSlotId;
		/// <summary>
		/// The item identifier, inventory items should specify their InstanceId
		/// here when slotting themselves.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> ItemId;

		[JsonIgnore]
		public abstract SlotTypes SlotType { get; }
		[JsonIgnore]
		public virtual bool IsFillable { get { return true; } }
		[JsonIgnore]
		public bool IsFilled { get { return IsFillable && !string.IsNullOrEmpty(ItemId); } }

		public abstract bool CanSlot(InventoryTypes inventoryType);

		public ModuleSlotModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			SlotId = new ListenerProperty<string>(value => slotId = value, () => slotId);
			ParentSlotId = new ListenerProperty<string>(value => parentSlotId = value, () => parentSlotId);
			ItemId = new ListenerProperty<string>(value => itemId = value, () => itemId);
		}
	}
}