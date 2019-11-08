namespace LunraGames.SubLight.Models
{
	public class ButtonEncounterLogModel : EdgedEncounterLogModel<ButtonEdgeModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Button;

		public override bool RequiresFallbackLog => false;
		public override bool EditableDuration => false;
	}
}