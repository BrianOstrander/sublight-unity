using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class ModuleSlotModel : Model
	{
		[JsonProperty] string slotId;
		[JsonProperty] string itemId;

		[JsonIgnore]
		public readonly ListenerProperty<string> SlotId;
		[JsonIgnore]
		public readonly ListenerProperty<string> ItemId;

		[JsonIgnore]
		public abstract SlotTypes SlotType { get; }
		[JsonIgnore]
		public virtual bool IsFillable { get { return true; } }
		[JsonIgnore]
		public bool IsFilled { get { return IsFillable && !string.IsNullOrEmpty(ItemId); } }

		public ModuleSlotModel()
		{
			SlotId = new ListenerProperty<string>(value => slotId = value, () => slotId);
			ItemId = new ListenerProperty<string>(value => itemId = value, () => itemId);
		}
	}
}