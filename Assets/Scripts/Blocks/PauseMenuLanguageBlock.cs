using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct PauseMenuLanguageBlock
	{
		public LanguageStringModel Title;
		public LanguageStringModel Resume;

		public LanguageStringModel Save;
		public DialogLanguageBlock SaveDisabledDuringEncounter;
		public DialogLanguageBlock SaveDisabledAlreadySaved;
		public DialogLanguageBlock SaveDisabledUnknown;

		public LanguageStringModel MainMenu;
		public DialogLanguageBlock MainMenuConfirm;

		public LanguageStringModel Quit;
		public DialogLanguageBlock QuitConfirm;

		public LanguageStringModel SavingTitle;
		public LanguageStringModel SavingComplete;
		public DialogLanguageBlock SavingError;

		public LanguageStringModel ReturningToMainMenu;
		public LanguageStringModel Quiting;
	}
}