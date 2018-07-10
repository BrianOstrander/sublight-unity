using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class EncounterInfoModel : SaveModel
	{
		[JsonProperty] string encounterId;
		[JsonProperty] string name;
		[JsonProperty] string description;
		[JsonProperty] string[] completedEncountersRequired = new string[0];
		[JsonProperty] SystemTypes[] validSystems = new SystemTypes[0];
		[JsonProperty] BodyTypes[] validBodies = new BodyTypes[0];
		[JsonProperty] InventoryTypes[] validProbes = new InventoryTypes[0];
		[JsonProperty] InventoryTypes[] validCrews = new InventoryTypes[0];
		[JsonProperty] EncounterLogListModel logs = new EncounterLogListModel();

		[JsonIgnore]
		public readonly ListenerProperty<string> EncounterId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Description;
		[JsonIgnore]
		public readonly ListenerProperty<string[]> CompletedEncountersRequired;
		[JsonIgnore]
		public readonly ListenerProperty<SystemTypes[]> ValidSystems;
		[JsonIgnore]
		public readonly ListenerProperty<BodyTypes[]> ValidBodies;
		[JsonIgnore]
		public readonly ListenerProperty<InventoryTypes[]> ValidProbes;
		[JsonIgnore]
		public readonly ListenerProperty<InventoryTypes[]> ValidCrews;

		#region Shortcuts
		[JsonIgnore]
		public EncounterLogListModel Logs { get { return logs; } }
		#endregion

		public EncounterInfoModel()
		{
			SaveType = SaveTypes.EncounterInfo;
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			CompletedEncountersRequired = new ListenerProperty<string[]>(value => completedEncountersRequired = value, () => completedEncountersRequired);
			ValidSystems = new ListenerProperty<SystemTypes[]>(value => validSystems = value, () => validSystems);
			ValidBodies = new ListenerProperty<BodyTypes[]>(value => validBodies = value, () => validBodies);
			ValidProbes = new ListenerProperty<InventoryTypes[]>(value => validProbes = value, () => validProbes);
			ValidCrews = new ListenerProperty<InventoryTypes[]>(value => validCrews = value, () => validCrews);
		}
	}
}