using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<KeyValueEntryModel>
	{
		[JsonProperty] KeyValueEntryModel[] entries = new KeyValueEntryModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<KeyValueEntryModel[]> Entries;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.KeyValue; } }

		public override bool EditableDuration { get { return false; } }

		public KeyValueEncounterLogModel()
		{
			Entries = new ListenerProperty<KeyValueEntryModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public KeyValueEntryModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}