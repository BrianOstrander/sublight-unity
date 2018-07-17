using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class KeyValueEntryEncounterLogModel : Model
	{
		[JsonProperty] string keyValueId;
		[JsonProperty] KeyValueTargets target;
		[JsonProperty] string key;

		[JsonIgnore]
		public readonly ListenerProperty<string> KeyValueId;
		[JsonIgnore]
		public readonly ListenerProperty<KeyValueTargets> Target;
		[JsonIgnore]
		public readonly ListenerProperty<string> Key;

		[JsonIgnore]
		public abstract KeyValueEncounterLogTypes KeyValueType { get; }

		public KeyValueEntryEncounterLogModel()
		{
			KeyValueId = new ListenerProperty<string>(value => keyValueId = value, () => keyValueId);
			Target = new ListenerProperty<KeyValueTargets>(value => target = value, () => target);
			Key = new ListenerProperty<string>(value => key = value, () => key);
		}
	}
}