using System;

namespace LunraGames.SubLight.Models
{
	public struct SaveBlocker
	{
		public readonly LanguageStringModel Title;
		public readonly LanguageStringModel Message;

		public SaveBlocker(LanguageStringModel title, LanguageStringModel message)
		{
			Title = title;
			Message = message;
		}
	}

	public class SaveBlockerListModel : StackListModel<SaveBlocker>
	{
		LanguageStringModel defaultTitle;
		public readonly ListenerProperty<LanguageStringModel> DefaultTitle;
		LanguageStringModel defaultMessage;
		public readonly ListenerProperty<LanguageStringModel> DefaultMessage;

		public SaveBlockerListModel()
		{
			DefaultTitle = new ListenerProperty<LanguageStringModel>(value => defaultTitle = value, () => defaultTitle);
			DefaultMessage = new ListenerProperty<LanguageStringModel>(value => defaultMessage = value, () => defaultMessage);
		}

		public Action Push(DialogLanguageBlock language)
		{
			return Push(language.Message, language.Title);
		}

		public Action Push(
			LanguageStringModel message = null,
			LanguageStringModel title = null
		)
		{
			return Push(
				new SaveBlocker(
					title ?? DefaultTitle,
					message ?? DefaultMessage
				)
			);
		}
	}
}