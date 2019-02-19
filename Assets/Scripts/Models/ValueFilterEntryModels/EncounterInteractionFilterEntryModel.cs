using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterInteractionFilterEntryModel : ValueFilterEntryModel<string>
	{
		[JsonProperty] string encounterId;
		[JsonProperty] EncounterInteractionFilterOperations operation;

		[JsonIgnore]
		public ListenerProperty<string> EncounterId;
		[JsonIgnore]
		public ListenerProperty<EncounterInteractionFilterOperations> Operation;

		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.EncounterInteraction; } }
		public override KeyValueTypes FilterValueType { get { return KeyValueTypes.String; } }

		public EncounterInteractionFilterEntryModel()
		{
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			Operation = new ListenerProperty<EncounterInteractionFilterOperations>(value => operation = value, () => operation);
		}
	}
}