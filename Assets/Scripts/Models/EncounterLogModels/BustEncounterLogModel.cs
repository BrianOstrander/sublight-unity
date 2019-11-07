using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class BustEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<BustEntryModel>
	{
		[JsonProperty] BustEntryModel[] entries = new BustEntryModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<BustEntryModel[]> Entries;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Bust; } }

		public override bool EditableDuration { get { return false; } }

		public BustEncounterLogModel()
		{
			Entries = new ListenerProperty<BustEntryModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public BustEntryModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}