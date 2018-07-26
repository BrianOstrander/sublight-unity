using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class ModuleSlotModel : Model
	{
		[JsonProperty] int index;
		[JsonProperty] string slotId;
		[JsonProperty] string itemId;

		/// <summary>
		/// The order these appear in the editor, not used in game for anything
		/// meaningful.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<int> Index;
		/// <summary>
		/// The slot identifier, inventory items should specify this in their
		/// SlotId to slot themselves here.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> SlotId;
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
			ItemId = new ListenerProperty<string>(value => itemId = value, () => itemId);
		}
	}
}