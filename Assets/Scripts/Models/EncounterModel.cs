using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class EncounterModel : Model
	{
		[JsonProperty] string encounterId;
		[JsonProperty] string instanceId;
		[JsonProperty] string name;
		[JsonProperty] string description;
		[JsonProperty] string[] completedEncountersRequired = new string[0];
		[JsonProperty] SystemTypes[] validSystems = new SystemTypes[0];
		[JsonProperty] BodyTypes[] validBodies = new BodyTypes[0];
		[JsonProperty] InventoryTypes[] validProbes = new InventoryTypes[0];
		[JsonProperty] InventoryTypes[] validCrews = new InventoryTypes[0];

		[JsonIgnore]
		public readonly ListenerProperty<string> EncounterId;
		[JsonIgnore]
		public readonly ListenerProperty<string> InstanceId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Description;

		public EncounterModel()
		{
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			InstanceId = new ListenerProperty<string>(value => instanceId = value, () => instanceId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
		}
	}
}