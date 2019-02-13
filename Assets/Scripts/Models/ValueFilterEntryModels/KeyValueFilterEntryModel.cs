using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class KeyValueFilterEntryModel<T> : ValueFilterEntryModel<T>, IKeyValueFilterEntryModel
	{
		[JsonProperty] KeyValueTargets target;
		[JsonIgnore] public ListenerProperty<KeyValueTargets> Target;

		[JsonProperty] string key;
		[JsonIgnore] public ListenerProperty<string> Key;

		[JsonProperty] KeyValueSources source;
		[JsonIgnore] public ListenerProperty<KeyValueSources> Source;

		[JsonProperty] KeyValueTargets sourceTarget;
		[JsonIgnore] public ListenerProperty<KeyValueTargets> SourceTarget;

		[JsonProperty] string sourceKey;
		[JsonIgnore] public ListenerProperty<string> SourceKey;

		[JsonIgnore]
		public KeyValueTargets FilterTarget
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
		[JsonIgnore]
		public KeyValueSources FilterSource
		{
			get { return Source.Value; }
			set { Source.Value = value; }
		}
		[JsonIgnore]
		public KeyValueTargets FilterSourceTarget
		{
			get { return SourceTarget.Value; }
			set { SourceTarget.Value = value; }
		}
		[JsonIgnore]
		public string FilterSourceKey
		{
			get { return SourceKey.Value; }
			set { SourceKey.Value = value; }
		}

		[JsonIgnore]
		public abstract KeyValueTypes FilterKeyValueType { get; }

		public KeyValueFilterEntryModel()
		{
			Target = new ListenerProperty<KeyValueTargets>(value => target = value, () => target);
			Key = new ListenerProperty<string>(value => key = value, () => key);

			Source = new ListenerProperty<KeyValueSources>(value => source = value, () => source);

			SourceTarget = new ListenerProperty<KeyValueTargets>(value => sourceTarget = value, () => sourceTarget);
			SourceKey = new ListenerProperty<string>(value => sourceKey = value, () => sourceKey);
		}
	}

	public interface IKeyValueFilterEntryModel : IValueFilterEntryModel
	{
		KeyValueTargets FilterTarget { get; set; }
		string FilterKey { get; set; }

		KeyValueSources FilterSource { get; set; }

		KeyValueTargets FilterSourceTarget { get; set; }
		string FilterSourceKey { get; set; }

		KeyValueTypes FilterKeyValueType { get; }

	}
}