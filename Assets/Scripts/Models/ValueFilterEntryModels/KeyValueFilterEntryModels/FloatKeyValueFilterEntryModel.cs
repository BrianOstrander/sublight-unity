using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class FloatKeyValueFilterEntryModel : KeyValueFilterEntryModel<float>
	{
		[JsonProperty] FloatFilterOperations operation;

		[JsonIgnore]
		public ListenerProperty<FloatFilterOperations> Operation;

		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.KeyValueFloat; } }
		public override KeyValueTypes FilterKeyValueType { get { return KeyValueTypes.Float; } }

		public FloatKeyValueFilterEntryModel()
		{
			Operation = new ListenerProperty<FloatFilterOperations>(value => operation = value, () => operation);
		}
	}
}