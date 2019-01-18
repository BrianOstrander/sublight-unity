using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncyclopediaEncounterLogModel : LinearEncounterLogModel, IEdgedEncounterLogModel<EncyclopediaEdgeModel>
	{
		[JsonProperty] EncyclopediaEdgeModel[] entries = new EncyclopediaEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<EncyclopediaEdgeModel[]> Entries;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Encyclopedia; } }

		public override bool EditableDuration { get { return false; } }

		public EncyclopediaEncounterLogModel()
		{
			Entries = new ListenerProperty<EncyclopediaEdgeModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public EncyclopediaEdgeModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}