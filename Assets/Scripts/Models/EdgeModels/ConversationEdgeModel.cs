using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ConversationEdgeModel : EdgeModel
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
						Behaviour = ConversationButtonPromptBehaviours.ButtonOnly
					};
				}
			}

			public ConversationButtonPromptBehaviours Behaviour;

			public string MessageOverride;
		}

		//[Serializable]
		//public struct AttachmentBlock
		//{
		//	public static AttachmentBlock Default { get { return new AttachmentBlock(); } }

		//}

		[JsonProperty] ConversationTypes conversationType;
		[JsonIgnore] public readonly ListenerProperty<ConversationTypes> ConversationType;

		[JsonProperty] string message;
		[JsonIgnore] public readonly ListenerProperty<string> Message;

		[JsonProperty] MessageBlock messageInfo;
		[JsonIgnore] public readonly ListenerProperty<MessageBlock> MessageInfo;

		[JsonProperty] PromptBlock promptInfo;
		[JsonIgnore] public readonly ListenerProperty<PromptBlock> PromptInfo;

		public override string EdgeName => ConversationType.Value.ToString();

		public override float Indent
		{
			get
			{
				switch (ConversationType.Value)
				{
					case ConversationTypes.MessageOutgoing:
					case ConversationTypes.Prompt:
						return DefaultIndent;
				}
				return 0f;
			}
		}
		
		public ConversationEdgeModel()
		{
			ConversationType = new ListenerProperty<ConversationTypes>(value => conversationType = value, () => conversationType);
			Message = new ListenerProperty<string>(value => message = value, () => message);

			MessageInfo = new ListenerProperty<MessageBlock>(value => messageInfo = value, () => messageInfo);
			PromptInfo = new ListenerProperty<PromptBlock>(value => promptInfo = value, () => promptInfo);
		}
	}
}