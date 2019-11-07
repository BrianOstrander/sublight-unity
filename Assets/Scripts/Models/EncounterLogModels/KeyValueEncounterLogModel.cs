using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<KeyValueEdgeModel>
	{
		[JsonProperty] KeyValueEdgeModel[] entries = new KeyValueEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<KeyValueEdgeModel[]> Entries;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.KeyValue; } }

		public override bool EditableDuration { get { return false; } }

		public KeyValueEncounterLogModel()
		{
			Entries = new ListenerProperty<KeyValueEdgeModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public KeyValueEdgeModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}