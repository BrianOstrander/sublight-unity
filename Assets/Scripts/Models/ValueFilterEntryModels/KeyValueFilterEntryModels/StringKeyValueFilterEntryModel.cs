using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class StringKeyValueFilterEntryModel : KeyValueFilterEntryModel<string>
	{
		[JsonProperty] StringFilterOperations operation;

		[JsonIgnore]
		public ListenerProperty<StringFilterOperations> Operation;

		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.KeyValueString; } }
		public override KeyValueTypes FilterKeyValueType { get { return KeyValueTypes.String; } }

		public StringKeyValueFilterEntryModel()
		{
			Operation = new ListenerProperty<StringFilterOperations>(value => operation = value, () => operation);
		}
	}
}