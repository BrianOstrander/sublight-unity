using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class InventoryModel : Model
	{
		[JsonProperty] bool ignore;
		[JsonProperty] float randomWeightMultiplier;
		[JsonProperty] string inventoryId;
		[JsonProperty] string instanceId;
		[JsonProperty] string name;
		[JsonProperty] string description;
		[JsonProperty] string parentSlotId;
		[JsonProperty] string[] tags = new string[0];

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
		public readonly ListenerProperty<bool> Ignore;
		/// <summary>
		/// This value is multiplied by the random weight. Higher values means
		/// this item will appear more often. The minimum is zero.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> RandomWeightMultiplier;
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
		[JsonIgnore]
		public readonly ListenerProperty<string[]> Tags;

		public InventoryModel()
		{
			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
			RandomWeightMultiplier = new ListenerProperty<float>(value => randomWeightMultiplier = value, () => randomWeightMultiplier);
			InventoryId = new ListenerProperty<string>(value => inventoryId = value, () => inventoryId);
			InstanceId = new ListenerProperty<string>(value => instanceId = value, () => instanceId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			ParentSlotId = new ListenerProperty<string>(value => parentSlotId = value, () => parentSlotId);
			Tags = new ListenerProperty<string[]>(value => tags = value, () => tags);
		}
	}
}