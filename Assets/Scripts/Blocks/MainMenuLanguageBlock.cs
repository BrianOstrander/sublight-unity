using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct MainMenuLanguageBlock
	{
		public LanguageStringModel NewGame;
		public DialogLanguageBlock NewGameOverwriteConfirm;
		public DialogLanguageBlock NewGameError;

		public LanguageStringModel ContinueGame;
		public DialogLanguageBlock ContinueGameError;

		public LanguageStringModel Settings;
		public LanguageStringModel Credits;

		public LanguageStringModel Quit;
		public DialogLanguageBlock QuitConfirm;
	}
}