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
		
		[JsonProperty] bool isHaltingOnClose;
		[JsonIgnore] public readonly ListenerProperty<bool> IsHaltingOnClose;

		public ModuleSwapEncounterLogModel()
		{
			Style = new ListenerProperty<Styles>(value => style = value, () => style);
			IsHaltingOnClose = new ListenerProperty<bool>(value => isHaltingOnClose = value, () => isHaltingOnClose);
		}
	}
}