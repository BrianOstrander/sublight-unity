using System.Collections.Generic;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct PauseMenuLanguageBlock
	{
		public struct DialogEntry
		{
			public LanguageStringModel Title;
			public LanguageStringModel Message;
		}

		public LanguageStringModel Title;
		public LanguageStringModel Resume;

		public LanguageStringModel Save;
		public DialogEntry SaveDisabledDuringEncounter;
		public DialogEntry SaveDisabledAlreadySaved;
		public DialogEntry SaveDisabledUnknown;

		public LanguageStringModel MainMenu;
		public DialogEntry MainMenuConfirm;

		public LanguageStringModel Quit;
		public DialogEntry QuitConfirm;

		public LanguageStringModel SavingTitle;
		public LanguageStringModel SavingComplete;
		public DialogEntry SavingError;
	}
}