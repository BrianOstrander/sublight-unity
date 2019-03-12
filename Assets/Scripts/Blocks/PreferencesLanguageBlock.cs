using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct PreferencesLanguageBlock
	{
		public LanguageStringModel Title;
		public LanguageStringModel Back;

		public ToggleOptionLanguageBlock Analytics;
		public ToggleOptionLanguageBlock Tutorial;
		public ToggleOptionLanguageBlock InterfaceLarge;
		public ToggleOptionLanguageBlock DemoMode;

		public LanguageStringModel VersionPrefix;
		public LanguageStringModel Gameplay;

		public DialogLanguageBlock BlockedDuringGame;
		public DialogLanguageBlock ReloadHomeRequired;
		public DialogLanguageBlock RestartRequired;

		public LanguageStringModel ReloadRestartConfirm;
		public LanguageStringModel ReloadRestartDiscard;
		public LanguageStringModel ReloadRestartCancel;
	}
}