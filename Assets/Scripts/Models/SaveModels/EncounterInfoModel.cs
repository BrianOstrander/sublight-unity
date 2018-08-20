using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterInfoModel : SaveModel
	{
		[JsonProperty] float orderWeight;
		[JsonProperty] bool hidden;
		[JsonProperty] string encounterId;
		[JsonProperty] string name;
		[JsonProperty] string description;
		[JsonProperty] string hook;
		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonProperty] SystemTypes[] validSystems = new SystemTypes[0];
		[JsonProperty] bool assignedToBody;
		[JsonProperty] BodyTypes[] validBodies = new BodyTypes[0];
		[JsonProperty] EncounterLogListModel logs = new EncounterLogListModel();

		/// <summary>
		/// Used to bias the selection of this encounter. The higher the weight
		/// the more likely it is to appear first.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> OrderWeight;
		/// <summary>
		/// If true, this encounter info will never show up in game.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> Hidden;
		/// <summary>
		/// The encounter identifier.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> EncounterId;
		/// <summary>
		/// The name of this encounter, will be seen by the player and listed as
		/// the meta data.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		/// <summary>
		/// An internal description of this encounter, only for editor use.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> Description;
		/// <summary>
		/// The hook that appears when entering a system. Seen by the player.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<string> Hook;
		[JsonIgnore]
		public readonly ListenerProperty<SystemTypes[]> ValidSystems;
		/// <summary>
		/// If true, this encounter gets associated with a specific body in a
		/// system.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> AssignedToBody;
		/// <summary>
		/// A system requires at least one of these bodies in order for the
		/// encounter to be assigned to it. If empty, there are no body
		/// requirements.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<BodyTypes[]> ValidBodies;

		#region Shortcuts
		[JsonIgnore]
		public ValueFilterModel Filtering { get { return filtering; } }
		[JsonIgnore]
		public EncounterLogListModel Logs { get { return logs; } }
		#endregion

		#region Utility
		/// <summary>
		/// If true, this encounter has to be in a system with certain types of
		/// bodies. Basically true if there are any entries in ValidBodies.
		/// </summary>
		/// <value><c>true</c> if has body requirements; otherwise, <c>false</c>.</value>
		[JsonIgnore]
		public bool HasBodyRequirements { get { return ValidBodies.Value.Any(); } }
		#endregion

		public EncounterInfoModel()
		{
			SaveType = SaveTypes.EncounterInfo;
			OrderWeight = new ListenerProperty<float>(value => orderWeight = value, () => orderWeight);
			Hidden = new ListenerProperty<bool>(value => hidden = value, () => hidden);
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			Hook = new ListenerProperty<string>(value => hook = value, () => hook);
			ValidSystems = new ListenerProperty<SystemTypes[]>(value => validSystems = value, () => validSystems);
			AssignedToBody = new ListenerProperty<bool>(value => assignedToBody = value, () => assignedToBody);
			ValidBodies = new ListenerProperty<BodyTypes[]>(value => validBodies = value, () => validBodies);
		}
	}
}