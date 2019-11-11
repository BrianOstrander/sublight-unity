using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ModuleSwapEncounterLogModel : EdgedEncounterLogModel<ModuleSwapEdgeModel>
	{
		public enum Styles
		{
			Unknown = 0,
			Instant = 10,
			Derelict = 20
		}
		
		public override EncounterLogTypes LogType => EncounterLogTypes.ModuleSwap;
		public override bool EditableDuration => false;
		
		[JsonProperty] Styles style;
		[JsonIgnore] public readonly ListenerProperty<Styles> Style;

		public ModuleSwapEncounterLogModel()
		{
			Style = new ListenerProperty<Styles>(value => style = value, () => style);
		}
	}
}