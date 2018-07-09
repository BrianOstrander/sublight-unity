using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class EncounterLogModel : Model
	{
		[JsonProperty] string logId;
		[JsonProperty] string instanceId;

		[JsonIgnore]
		public readonly ListenerProperty<string> LogId;
		[JsonIgnore]
		public readonly ListenerProperty<string> InstanceId;

		public abstract EncounterLogTypes LogType { get; }

		public EncounterLogModel()
		{
			LogId = new ListenerProperty<string>(value => logId = value, () => logId);
			InstanceId = new ListenerProperty<string>(value => instanceId = value, () => instanceId);
		}
	}
}