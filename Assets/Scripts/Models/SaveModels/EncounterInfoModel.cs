using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterInfoModel : SaveModel
	{
		[JsonProperty] int orderWeight;
		/// <summary>
		/// Used to bias the selection of this encounter. The higher the weight
		/// the more likely it is to appear first.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<int> OrderWeight;

		[JsonProperty] float randomWeightMultiplier;
		/// <summary>
		/// This value is multiplied by the random weight. Higher values means
		/// this encounter will appear more often. The minimum is zero.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<float> RandomWeightMultiplier;

		[JsonProperty] float randomAppearance;
		/// <summary>
		/// The random appearance of this encounter, if 1 then always, if less
		/// than 1 there's a chance it will not be considered.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<float> RandomAppearance;

		[JsonProperty] string encounterId;
		/// <summary>
		/// The encounter identifier.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<string> EncounterId;

		[JsonProperty] string name;
		/// <summary>
		/// The name of this encounter, will be seen by the player and listed as
		/// the meta data.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<string> Name;

		[JsonProperty] string description;
		/// <summary>
		/// An internal description of this encounter, only for editor use.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<string> Description;

		[JsonProperty] EncounterTriggers trigger;
		/// <summary>
		/// The encounter trigger, does in interrupt the player, or appear in
		/// above a body in the system viewer.
		/// </summary>
		[JsonIgnore] public readonly ListenerProperty<EncounterTriggers> Trigger;

		[JsonProperty] string defaultEndLogId;
		/// <summary>
		/// The default id for quickly ending an encounter.
		/// </summary>
		/// <remarks>
		/// Mostly used to quickly bail out without having to create a specific
		/// log just to end the encounter.
		/// </remarks>
		[JsonIgnore] public readonly ListenerProperty<string> DefaultEndLogId;

		#region Serialized Models
		[JsonProperty] ValueFilterModel filtering = ValueFilterModel.Default();
		[JsonIgnore] public ValueFilterModel Filtering { get { return filtering; } }

		[JsonProperty] EncounterLogListModel logs = new EncounterLogListModel();
		[JsonIgnore] public EncounterLogListModel Logs { get { return logs; } }
		#endregion

		public EncounterInfoModel()
		{
			SaveType = SaveTypes.EncounterInfo;
			OrderWeight = new ListenerProperty<int>(value => orderWeight = value, () => orderWeight);
			RandomWeightMultiplier = new ListenerProperty<float>(value => randomWeightMultiplier = value, () => randomWeightMultiplier);
			RandomAppearance = new ListenerProperty<float>(value => randomAppearance = value, () => randomAppearance);
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);
			Trigger = new ListenerProperty<EncounterTriggers>(value => trigger = value, () => trigger);
			DefaultEndLogId = new ListenerProperty<string>(value => defaultEndLogId = value, () => defaultEndLogId);
		}
	}
}