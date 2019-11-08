using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class BustEncounterLogModel : EdgedEncounterLogModel<BustEdgeModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Bust;
		public override bool EditableDuration => false;
	}
}