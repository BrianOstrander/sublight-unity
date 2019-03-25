using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct ToggleOptionLanguageBlock
	{
		public LanguageStringModel Message;

		public LanguageStringModel MessageEnabled;
		public LanguageStringModel MessageDisabled;

		public LanguageStringModel GetValue(bool value)
		{
			return value ? MessageEnabled : MessageDisabled;
		}

		public ToggleOptionLanguageBlock Duplicate(
			LanguageStringModel message = null,
			LanguageStringModel messageEnabled = null,
			LanguageStringModel messageDisabled = null
		)
		{
			return new ToggleOptionLanguageBlock
			{
				Message = message ?? Message,
				MessageEnabled = messageEnabled ?? MessageEnabled,
				MessageDisabled = messageDisabled ?? MessageDisabled
			};
		}
	}
}