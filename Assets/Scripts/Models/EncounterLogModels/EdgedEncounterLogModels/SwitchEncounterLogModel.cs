using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SwitchEncounterLogModel : EdgedEncounterLogModel<SwitchEdgeModel>
	{
		public enum SelectionMethods
		{
			Unknown = 0,
			FirstFilter = 10,
			Random = 20,
			RandomWeighted = 30
		}

		[JsonProperty] SelectionMethods selectionMethod;
		[JsonIgnore] public readonly ListenerProperty<SelectionMethods> SelectionMethod;

		public override EncounterLogTypes LogType => EncounterLogTypes.Switch;

		public override bool RequiresFallbackLog => false;
		public override bool EditableDuration => false;

		public SwitchEncounterLogModel()
		{
			SelectionMethod = new ListenerProperty<SelectionMethods>(value => selectionMethod = value, () => selectionMethod);
		}
	}
}