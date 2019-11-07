using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class BustEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<BustEdgeModel>
	{
		[JsonProperty] BustEdgeModel[] entries = new BustEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<BustEdgeModel[]> Entries;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Bust; } }

		public override bool EditableDuration { get { return false; } }

		public BustEncounterLogModel()
		{
			Entries = new ListenerProperty<BustEdgeModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public BustEdgeModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}