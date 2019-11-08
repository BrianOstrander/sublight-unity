namespace LunraGames.SubLight.Models
{
	public class ModuleEncounterLogModel : EdgedEncounterLogModel<ModuleEdgeModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Module;
		public override bool EditableDuration => false;
	}
}