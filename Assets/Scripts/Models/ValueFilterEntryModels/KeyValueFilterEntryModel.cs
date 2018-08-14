using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class KeyValueFilterEntryModel<T> : ValueFilterEntryModel<T>
	{
		[JsonProperty] KeyValueTargets target;
		[JsonProperty] string key;

		[JsonIgnore]
		public ListenerProperty<KeyValueTargets> Target;
		[JsonIgnore]
		public ListenerProperty<string> Key;

		public KeyValueFilterEntryModel()
		{
			Target = new ListenerProperty<KeyValueTargets>(value => target = value, () => target);
			Key = new ListenerProperty<string>(value => key = value, () => key);
		}
	}
}