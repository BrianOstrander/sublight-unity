using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct GameCompleteLanguageBlock
	{
		public LanguageStringModel FailureTitle;
		public LanguageStringModel FailureHeader;
		public LanguageStringModel FailureBody;

		public LanguageStringModel SuccessTitle;
		public LanguageStringModel SuccessHeader;
		public LanguageStringModel SuccessBody;

		public LanguageStringModel RetryFailure;
		public LanguageStringModel RetrySuccess;
		public LanguageStringModel MainMenu;

		public LanguageStringModel RetryTitle;
		public DialogLanguageBlock RetryError;

		public LanguageStringModel ReturningToMainMenu;
	}
}