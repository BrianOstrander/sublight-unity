using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EnumerationKeyValueFilterEntryModel : KeyValueFilterEntryModel<int>
	{
		[JsonProperty] EnumerationFilterOperations operation;

		[JsonIgnore]
		public ListenerProperty<EnumerationFilterOperations> Operation;

		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.KeyValueEnumeration; } }
		public override KeyValueTypes FilterValueType { get { return KeyValueTypes.Enumeration; } }

		public EnumerationKeyValueFilterEntryModel()
		{
			Operation = new ListenerProperty<EnumerationFilterOperations>(value => operation = value, () => operation);
		}
	}
}