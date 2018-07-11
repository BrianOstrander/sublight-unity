using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class EncounterLogModel : Model
	{
		[JsonProperty] bool beginning;
		[JsonProperty] bool ending;
		[JsonProperty] string logId;

		[JsonIgnore]
		public readonly ListenerProperty<bool> Beginning;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Ending;
		[JsonIgnore]
		public readonly ListenerProperty<string> LogId;

		[JsonIgnore]
		public abstract EncounterLogTypes LogType { get; }
		[JsonIgnore]
		public abstract string NextLog { get; }

		public EncounterLogModel()
		{
			Beginning = new ListenerProperty<bool>(value => beginning = value, () => beginning);
			Ending = new ListenerProperty<bool>(value => ending = value, () => ending);
			LogId = new ListenerProperty<string>(value => logId = value, () => logId);
		}
	}
}