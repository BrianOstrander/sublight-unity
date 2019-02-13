using System;
using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SaveBlockerListModel : Model
	{
		#region Serialized
		SaveBlocker[] blockers = new SaveBlocker[0];
		readonly ListenerProperty<SaveBlocker[]> blockerListener;
		public readonly ReadonlyProperty<SaveBlocker[]> Blockers;

		LanguageStringModel defaultTitle;
		public readonly ListenerProperty<LanguageStringModel> DefaultTitle;
		LanguageStringModel defaultMessage;
		public readonly ListenerProperty<LanguageStringModel> DefaultMessage;
		#endregion

		public SaveBlockerListModel()
		{
			Blockers = new ReadonlyProperty<SaveBlocker[]>(value => blockers = value, () => blockers, out blockerListener);
			DefaultTitle = new ListenerProperty<LanguageStringModel>(value => defaultTitle = value, () => defaultTitle);
			DefaultMessage = new ListenerProperty<LanguageStringModel>(value => defaultMessage = value, () => defaultMessage);
		}

		public bool CanSave { get { return Blockers.Value.Length == 0; } }

		public SaveBlocker Peek() { return Blockers.Value.FirstOrDefault(); }

		public Action Push(DialogLanguageBlock language)
		{
			return Push(language.Message, language.Title);
		}

		public Action Push(
			LanguageStringModel message = null,
			LanguageStringModel title = null
		)
		{
			var blocker = new SaveBlocker(
				title ?? DefaultTitle,
				message ?? DefaultMessage
			);
			blockerListener.Value = blockerListener.Value.Prepend(blocker).ToArray();

			return GetRemoveCallback(blocker.Id);
		}

		Action GetRemoveCallback(string id)
		{
			return () =>
			{
				blockerListener.Value = blockerListener.Value.Where(b => b.Id != id).ToArray();
			};
		}
	}
}