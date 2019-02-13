using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct SaveBlocker
	{
		public readonly string Id;
		public readonly LanguageStringModel Title;
		public readonly LanguageStringModel Message;

		public SaveBlocker(LanguageStringModel title, LanguageStringModel message)
		{
			Id = Guid.NewGuid().ToString();
			Title = title;
			Message = message;
		}
	}
}