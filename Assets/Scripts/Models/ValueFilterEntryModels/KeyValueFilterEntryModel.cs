using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class KeyValueFilterEntryModel<T> : ValueFilterEntryModel<T>, IKeyValueFilterEntryModel
	{
		[JsonProperty] KeyValueTargets target;
		[JsonProperty] string key;

		[JsonIgnore]
		public ListenerProperty<KeyValueTargets> Target;
		[JsonIgnore]
		public ListenerProperty<string> Key;

		[JsonIgnore]
		public KeyValueTargets FilterKeyTarget
		{
			get { return Target.Value; }
			set { Target.Value = value; }
		}
		[JsonIgnore]
		public string FilterKey
		{
			get { return Key.Value; }
			set { Key.Value = value; }
		}

		public KeyValueFilterEntryModel()
		{
			Target = new ListenerProperty<KeyValueTargets>(value => target = value, () => target);
			Key = new ListenerProperty<string>(value => key = value, () => key);
		}
	}

	public interface IKeyValueFilterEntryModel : IValueFilterEntryModel
	{
		KeyValueTargets FilterKeyTarget { get; set; }
		string FilterKey { get; set; }
	}
}