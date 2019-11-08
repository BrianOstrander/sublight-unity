using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class DialogEncounterLogModel : EdgedEncounterLogModel<DialogEdgeModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Dialog;

		public override bool RequiresFallbackLog => false;
		public override bool EditableDuration => false;
	}
}