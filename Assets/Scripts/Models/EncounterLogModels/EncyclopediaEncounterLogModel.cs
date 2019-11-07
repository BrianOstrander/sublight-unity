using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncyclopediaEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<EncyclopediaEntryModel>
	{
		[JsonProperty] EncyclopediaEntryModel[] entries = new EncyclopediaEntryModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<EncyclopediaEntryModel[]> Entries;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Encyclopedia; } }

		public override bool EditableDuration { get { return false; } }

		public EncyclopediaEncounterLogModel()
		{
			Entries = new ListenerProperty<EncyclopediaEntryModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public EncyclopediaEntryModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}