using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<ButtonEdgeModel>
	{
		[Serializable]
		public struct ConversationStyleBlock
		{
			public static ConversationStyleBlock Default
			{
				get
				{
					return new ConversationStyleBlock
					{
						Theme = ConversationThemes.Foreigner
					};
				}
			}

			public ConversationThemes Theme;
		}

		[JsonProperty] ButtonEdgeModel[] buttons = new ButtonEdgeModel[0];

		[JsonProperty] ConversationButtonStyles style;

		[JsonProperty] ConversationStyleBlock conversationStyle;

		[JsonIgnore]
		public readonly ListenerProperty<ButtonEdgeModel[]> Buttons;

		[JsonIgnore]
		public readonly ListenerProperty<ConversationButtonStyles> Style;

		[JsonIgnore]
		public readonly ListenerProperty<ConversationStyleBlock> ConversationStyle;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Button; } }

		public override bool RequiresNextLog { get { return false; } }
		public override bool EditableDuration { get { return false; } }

		public ButtonEncounterLogModel()
		{
			Buttons = new ListenerProperty<ButtonEdgeModel[]>(value => buttons = value, () => buttons);

			Style = new ListenerProperty<ConversationButtonStyles>(value => style = value, () => style);

			ConversationStyle = new ListenerProperty<ConversationStyleBlock>(value => conversationStyle = value, () => conversationStyle);
		}

		[JsonIgnore]
		public ButtonEdgeModel[] Edges
		{
			get { return Buttons.Value; }
			set { Buttons.Value = value; }
		}
	}
}