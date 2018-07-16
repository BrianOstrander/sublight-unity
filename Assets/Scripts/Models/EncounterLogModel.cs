using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public abstract class EncounterLogModel : Model
	{
		[JsonProperty] int index;
		[JsonProperty] bool beginning;
		[JsonProperty] bool ending;
		[JsonProperty] string logId;
		[JsonProperty] float duration;

		/// <summary>
		/// The order these appear in the editor, not used in game for anything
		/// meaningful.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<int> Index;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Beginning;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Ending;
		[JsonIgnore]
		public readonly ListenerProperty<string> LogId;
		[JsonIgnore]
		public readonly ListenerProperty<float> Duration;

		[JsonIgnore]
		public abstract EncounterLogTypes LogType { get; }
		[JsonIgnore]
		public abstract string NextLog { get; }

		[JsonIgnore]
		public virtual bool EditableDuration { get { return true; } }
		[JsonIgnore]
		public virtual float TotalDuration { get { return Duration.Value; } }

		public EncounterLogModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Beginning = new ListenerProperty<bool>(value => beginning = value, () => beginning);
			Ending = new ListenerProperty<bool>(value => ending = value, () => ending);
			LogId = new ListenerProperty<string>(value => logId = value, () => logId);
			Duration = new ListenerProperty<float>(value => duration = value, () => duration);
		}
	}
}