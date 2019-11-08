using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncyclopediaEncounterLogModel : EdgedEncounterLogModel<EncyclopediaEdgeModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Encyclopedia;
		public override bool EditableDuration => false;
	}
}