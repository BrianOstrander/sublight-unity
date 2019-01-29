using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ConversationEntryModel : EdgeEntryModel
	{
		[Serializable]
		public struct MessageBlock
		{
			public static MessageBlock Default { get { return new MessageBlock(); } }

			// Maybe I wanna add something here? I dunno!
		}

		[Serializable]
		public struct PromptBlock
		{
			public static PromptBlock Default
			{
				get
				{
					return new PromptBlock
					{
						Style = ConversationButtonStyles.Conversation,
						Theme = ConversationThemes.Crew
					};
				}
			}

			public ConversationButtonStyles Style;
			public ConversationThemes Theme;
		}

		//[Serializable]
		//public struct AttachmentBlock
		//{
		//	public static AttachmentBlock Default { get { return new AttachmentBlock(); } }

		//}

		[JsonProperty] ConversationTypes conversationType;
		[JsonProperty] string message;

		[JsonProperty] MessageBlock messageInfo;
		[JsonProperty] PromptBlock promptInfo;

		[JsonIgnore]
		public readonly ListenerProperty<ConversationTypes> ConversationType;
		[JsonIgnore]
		public readonly ListenerProperty<string> Message;

		[JsonIgnore]
		public readonly ListenerProperty<MessageBlock> MessageInfo;
		[JsonIgnore]
		public readonly ListenerProperty<PromptBlock> PromptInfo;

		public ConversationEntryModel()
		{
			ConversationType = new ListenerProperty<ConversationTypes>(value => conversationType = value, () => conversationType);
			Message = new ListenerProperty<string>(value => message = value, () => message);

			MessageInfo = new ListenerProperty<MessageBlock>(value => messageInfo = value, () => messageInfo);
			PromptInfo = new ListenerProperty<PromptBlock>(value => promptInfo = value, () => promptInfo);
		}
	}
}