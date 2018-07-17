using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class SetStringEntryEncounterLogModel : KeyValueEntryEncounterLogModel
	{
		[JsonProperty] string value;

		[JsonIgnore]
		public readonly ListenerProperty<string> Value;

		public override KeyValueEncounterLogTypes KeyValueType { get { return KeyValueEncounterLogTypes.SetString; } }

		public SetStringEntryEncounterLogModel()
		{
			Value = new ListenerProperty<string>(value => this.value = value, () => value);
		}
	}
}