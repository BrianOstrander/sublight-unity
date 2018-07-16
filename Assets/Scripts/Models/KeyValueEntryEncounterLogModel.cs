using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class KeyValueEntryEncounterLogModel : Model
	{
		[JsonProperty] KeyValueEncounterLogTargets target;
		[JsonProperty] string key;

		[JsonIgnore]
		public readonly ListenerProperty<KeyValueEncounterLogTargets> Target;
		[JsonIgnore]
		public readonly ListenerProperty<string> Key;

		[JsonIgnore]
		public abstract KeyValueEncounterLogTypes StateType { get; }

		public KeyValueEntryEncounterLogModel()
		{
			Target = new ListenerProperty<KeyValueEncounterLogTargets>(value => target = value, () => target);
			Key = new ListenerProperty<string>(value => key = value, () => key);
		}
	}
}