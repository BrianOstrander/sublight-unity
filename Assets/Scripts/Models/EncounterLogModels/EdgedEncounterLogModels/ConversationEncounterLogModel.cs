namespace LunraGames.SubLight.Models
{
	public class ConversationEncounterLogModel : EdgedEncounterLogModel<ConversationEdgeModel>
	{
		public override EncounterLogTypes LogType => EncounterLogTypes.Conversation;
		public override bool EditableDuration => false;
	}
}