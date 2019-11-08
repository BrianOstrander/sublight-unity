using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class KeyValueEncounterLogModel : EdgedEncounterLogModel<KeyValueEdgeModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.KeyValue;
		public override bool EditableDuration => false;
	}
}