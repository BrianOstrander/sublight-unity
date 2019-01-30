using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class EncounterLogModel : Model
	{
		[JsonProperty] int index;
		[JsonProperty] bool beginning;
		[JsonProperty] bool ending;
		[JsonProperty] string logId;
		[JsonProperty] float duration;

		[JsonProperty] string fallbackLogId;

		[JsonProperty] string name;
		[JsonProperty] string notes;

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
		public readonly ListenerProperty<string> FallbackLogId;

		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Notes;

		[JsonIgnore]
		public abstract EncounterLogTypes LogType { get; }
		[JsonIgnore]
		public virtual string NextLog { get { return FallbackLogId.Value; } }
		[JsonIgnore]
		public virtual bool CanFallback { get { return true; } }

		/// <summary>
		/// If this value returns true, not having a next log is an error,
		/// otherwise just a warning will appear in the editor.
		/// </summary>
		/// <value><c>true</c> if requires next log; otherwise, <c>false</c>.</value>
		[JsonIgnore]
		public virtual bool RequiresFallbackLog { get { return true; } }
		[JsonIgnore]
		public virtual bool EditableDuration { get { return true; } }
		[JsonIgnore]
		public virtual float TotalDuration { get { return Duration.Value; } }
		[JsonIgnore]
		public bool HasNotes { get { return !string.IsNullOrEmpty(Notes.Value); } }
		[JsonIgnore]
		public bool HasName { get { return !string.IsNullOrEmpty(Name.Value); } }

		public EncounterLogModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Beginning = new ListenerProperty<bool>(value => beginning = value, () => beginning);
			Ending = new ListenerProperty<bool>(value => ending = value, () => ending);
			LogId = new ListenerProperty<string>(value => logId = value, () => logId);
			Duration = new ListenerProperty<float>(value => duration = value, () => duration);

			FallbackLogId = new ListenerProperty<string>(value => fallbackLogId = value, () => fallbackLogId);

			Name = new ListenerProperty<string>(value => name = value, () => name);
			Notes = new ListenerProperty<string>(value => notes = value, () => notes);
		}
	}
}