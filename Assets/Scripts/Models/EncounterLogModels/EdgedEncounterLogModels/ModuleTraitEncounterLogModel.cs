namespace LunraGames.SubLight.Models
{
	public class ModuleTraitEncounterLogModel : EdgedEncounterLogModel<ModuleTraitEdgeModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.ModuleTrait;
		public override bool EditableDuration => false;
	}
}