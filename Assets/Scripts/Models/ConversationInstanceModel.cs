using System;

using UnityEngine;

using Newtonsoft.Json;
using LunraGames.SubLight.Views; // <- ignore this I guess... i dunno...

namespace LunraGames.SubLight.Models
{
	public class ConversationInstanceModel : Model
	{
		[SerializeField] string bustId;
		[SerializeField] bool isFocused;

		Action<bool> show;
		Action<bool> close;
		Action destroy;

		Func<bool> isShown;
		Func<bool> isClosed;
		Func<bool> isDestroyed;

		Action<ConversationButtonStyles, ConversationThemes, ConversationButtonBlock> onPrompt;

		[JsonIgnore]
		public readonly ListenerProperty<string> BustId;
		[JsonIgnore]
		public readonly ListenerProperty<bool> IsFocused;

		[JsonIgnore]
		public readonly ListenerProperty<Action<bool>> Show;
		[JsonIgnore]
		public readonly ListenerProperty<Action<bool>> Close;
		[JsonIgnore]
		public readonly ListenerProperty<Action> Destroy;

		[JsonIgnore]
		public readonly ListenerProperty<Func<bool>> IsShown;
		[JsonIgnore]
		public readonly ListenerProperty<Func<bool>> IsClosed;
		[JsonIgnore]
		public readonly ListenerProperty<Func<bool>> IsDestroyed;

		[JsonIgnore]
		public readonly ListenerProperty<Action<ConversationButtonStyles, ConversationThemes, ConversationButtonBlock>> OnPrompt;

		public ConversationInstanceModel()
		{
			BustId = new ListenerProperty<string>(value => bustId = value, () => bustId);
			IsFocused = new ListenerProperty<bool>(value => isFocused = value, () => isFocused);

			Show = new ListenerProperty<Action<bool>>(value => show = value, () => show);
			Close = new ListenerProperty<Action<bool>>(value => close = value, () => close);
			Destroy = new ListenerProperty<Action>(value => destroy = value, () => destroy);

			IsShown = new ListenerProperty<Func<bool>>(value => isShown = value, () => isShown);
			IsClosed = new ListenerProperty<Func<bool>>(value => isClosed = value, () => isClosed);
			IsDestroyed = new ListenerProperty<Func<bool>>(value => isDestroyed = value, () => isDestroyed);

			OnPrompt = new ListenerProperty<Action<ConversationButtonStyles, ConversationThemes, ConversationButtonBlock>>(value => onPrompt = value, () => onPrompt);
		}
	}
}