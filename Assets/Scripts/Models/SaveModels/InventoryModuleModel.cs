using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class InventoryModuleModel : SaveModel
	{
		[JsonProperty] bool ignore;
		[JsonProperty] string inventoryId;
		[JsonProperty] string instanceId;
		[JsonProperty] string name;
		[JsonProperty] string description;
		[JsonProperty] string[] tags = new string[0];

		[JsonIgnore]
		public readonly ListenerProperty<bool> Ignore;
		[JsonIgnore]
		public readonly ListenerProperty<string> InventoryId;
		[JsonIgnore]
		public readonly ListenerProperty<string> InstanceId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Description;
		[JsonIgnore]
		public readonly ListenerProperty<string[]> Tags;

		public InventoryModuleModel()
		{
			SaveType = SaveTypes.InventoryModule;
			SiblingBehaviour = SiblingBehaviours.All;

			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
			InventoryId = new ListenerProperty<string>(value => inventoryId = value, () => inventoryId);
			InstanceId = new ListenerProperty<string>(value => instanceId = value, () => instanceId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			Tags = new ListenerProperty<string[]>(value => tags = value, () => tags);
		}
	}
}