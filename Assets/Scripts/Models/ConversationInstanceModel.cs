using System;

using UnityEngine;

using Newtonsoft.Json;
using LunraGames.SubLight.Views; // <- ignore this I guess... i dunno...

namespace LunraGames.SubLight.Models
{
	public class ConversationInstanceModel : Model
	{
		#region Serialized
		[SerializeField] string bustId;
		[JsonIgnore] public readonly ListenerProperty<string> BustId;
		#endregion

		[SerializeField] bool isFocused;
		[JsonIgnore] public readonly ListenerProperty<bool> IsFocused;

		Action<bool> show;
		[JsonIgnore] public readonly ListenerProperty<Action<bool>> Show;

		Action<bool> close;
		[JsonIgnore] public readonly ListenerProperty<Action<bool>> Close;

		Action destroy;
		[JsonIgnore] public readonly ListenerProperty<Action> Destroy;

		Func<bool> isShown;
		[JsonIgnore] public readonly ListenerProperty<Func<bool>> IsShown;

		Func<bool> isClosed;
		[JsonIgnore] public readonly ListenerProperty<Func<bool>> IsClosed;

		Func<bool> isDestroyed;
		[JsonIgnore] public readonly ListenerProperty<Func<bool>> IsDestroyed;

		Action<ConversationButtonBlock> onPrompt;
		[JsonIgnore] public readonly ListenerProperty<Action<ConversationButtonBlock>> OnPrompt;

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

			OnPrompt = new ListenerProperty<Action<ConversationButtonBlock>>(value => onPrompt = value, () => onPrompt);
		}
	}
}