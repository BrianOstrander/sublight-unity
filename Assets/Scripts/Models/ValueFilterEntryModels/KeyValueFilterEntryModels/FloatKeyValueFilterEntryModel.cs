using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class FloatKeyValueFilterEntryModel : KeyValueFilterEntryModel<float>
	{
		[JsonProperty] FloatFilterOperations operation;

		[JsonIgnore]
		public ListenerProperty<FloatFilterOperations> Operation;

		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.KeyValueFloat; } }

		public FloatKeyValueFilterEntryModel()
		{
			Operation = new ListenerProperty<FloatFilterOperations>(value => operation = value, () => operation);
		}
	}
}