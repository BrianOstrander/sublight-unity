using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SwitchEncounterLogModel : LinearEncounterLogModel
	{
		[JsonProperty] EncounterLogSwitchEdgeModel[] switches = new EncounterLogSwitchEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<EncounterLogSwitchEdgeModel[]> Switches;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Switch; } }

		public override bool EditableDuration { get { return false; } }

		public SwitchEncounterLogModel()
		{
			Switches = new ListenerProperty<EncounterLogSwitchEdgeModel[]>(value => switches = value, () => switches);
		}
	}
}