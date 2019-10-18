using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterInteractionFilterEntryModel : ValidatedFilterEntryModel<string>
	{
		[JsonProperty] EncounterInteractionFilterOperations operation;
		[JsonIgnore] public ListenerProperty<EncounterInteractionFilterOperations> Operation;

		public override ValueFilterTypes FilterType => ValueFilterTypes.EncounterInteraction;
		public override KeyValueTypes FilterValueType => KeyValueTypes.String;
		
		public EncounterInteractionFilterEntryModel()
		{
			Operation = new ListenerProperty<EncounterInteractionFilterOperations>(value => operation = value, () => operation);
		}
	}
}