using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterEventEdgeModel : Model, IEdgeModel
	{
		[JsonProperty] EncounterEventEntryModel entry = new EncounterEventEntryModel();

		[JsonProperty] int index;
		[JsonProperty] bool ignore;

		[JsonIgnore]
		public EncounterEventEntryModel Entry { get { return entry; } }
		[JsonIgnore]
		public ListenerProperty<string> EventId { get { return Entry.EventId; } }

		[JsonIgnore]
		public readonly ListenerProperty<int> Index;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Ignore;


		public EncounterEventEdgeModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
		}

		[JsonIgnore]
		public string EdgeName { get { return Entry.EncounterEvent.Value.ToString(); } }
		[JsonIgnore]
		public int EdgeIndex
		{
			get { return Index.Value; }
			set { Index.Value = value; }
		}
		[JsonIgnore]
		public string EdgeId
		{
			get { return EventId.Value; }
			set { EventId.Value = value; }
		}
	}
}