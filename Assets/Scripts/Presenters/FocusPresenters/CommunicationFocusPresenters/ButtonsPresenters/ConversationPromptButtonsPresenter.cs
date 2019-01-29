using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ConversationPromptButtonsPresenter : ButtonsPresenter<IConversationButtonsView>
	{
		public override ConversationButtonStyles Style { get { return ConversationButtonStyles.Conversation; } }
	}
}