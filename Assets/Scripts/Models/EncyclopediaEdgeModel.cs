using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncyclopediaEdgeModel : Model, IEdgeModel
	{
		[JsonProperty] EncyclopediaEntryModel entry = new EncyclopediaEntryModel();

		[JsonProperty] int index;
		[JsonProperty] bool ignore;

		[JsonIgnore]
		public EncyclopediaEntryModel Entry { get { return entry; } }
		[JsonIgnore]
		public ListenerProperty<string> EncyclopediaId { get { return Entry.EncyclopediaId; } }

		[JsonIgnore]
		public readonly ListenerProperty<int> Index;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Ignore;


		public EncyclopediaEdgeModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Ignore = new ListenerProperty<bool>(value => ignore = value, () => ignore);
		}

		[JsonIgnore]
		public string EdgeName { get { return string.IsNullOrEmpty(entry.Header.Value) ? "Introduction" : "Section"; } }
		[JsonIgnore]
		public int EdgeIndex
		{
			get { return Index.Value; }
			set { Index.Value = value; }
		}
		[JsonIgnore]
		public string EdgeId
		{
			get { return EncyclopediaId.Value; }
			set { EncyclopediaId.Value = value; }
		}
	}
}