using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SwitchEncounterLogModel : LinearEncounterLogModel, IEdgedEncounterLogModel<SwitchEdgeModel>
	{
		[JsonProperty] SwitchEdgeModel[] switches = new SwitchEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<SwitchEdgeModel[]> Switches;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Switch; } }

		public override bool RequiresNextLog { get { return false; } }
		public override bool EditableDuration { get { return false; } }

		public SwitchEncounterLogModel()
		{
			Switches = new ListenerProperty<SwitchEdgeModel[]>(value => switches = value, () => switches);
		}

		[JsonIgnore]
		public SwitchEdgeModel[] Edges
		{
			get { return Switches.Value; }
			set { Switches.Value = value; }
		}
	}
}