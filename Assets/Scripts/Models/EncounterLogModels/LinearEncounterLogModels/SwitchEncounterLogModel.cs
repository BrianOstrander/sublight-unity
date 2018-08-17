using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SwitchEncounterLogModel : LinearEncounterLogModel
	{
		[JsonProperty] SwitchEdgeModel[] switches = new SwitchEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<SwitchEdgeModel[]> Switches;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Switch; } }

		public override bool EditableDuration { get { return false; } }

		public SwitchEncounterLogModel()
		{
			Switches = new ListenerProperty<SwitchEdgeModel[]>(value => switches = value, () => switches);
		}
	}
}