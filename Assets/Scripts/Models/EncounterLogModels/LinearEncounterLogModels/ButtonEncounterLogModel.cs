using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEncounterLogModel : LinearEncounterLogModel, IEdgedEncounterLogModel<ButtonEdgeModel>
	{
		public enum Styles
		{
			Unknown = 0,
			Conversation = 10
		}

		[Serializable]
		public struct ConversationStyleBlock
		{
			public static ConversationStyleBlock Default
			{
				get
				{
					return new ConversationStyleBlock
					{
						Theme = Themes.Foreigner
					};
				}
			}

			public enum Themes
			{
				Unknown = 0,
				Crew = 10,
				AwayTeam = 20,
				Foreigner = 30,
				Downlink = 40
			}

			public Themes Theme;
		}

		[JsonProperty] ButtonEdgeModel[] buttons = new ButtonEdgeModel[0];

		[JsonProperty] Styles style;

		[JsonProperty] ConversationStyleBlock conversationStyle;

		[JsonIgnore]
		public readonly ListenerProperty<ButtonEdgeModel[]> Buttons;

		[JsonIgnore]
		public readonly ListenerProperty<Styles> Style;

		[JsonIgnore]
		public readonly ListenerProperty<ConversationStyleBlock> ConversationStyle;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Button; } }

		public override bool RequiresNextLog { get { return false; } }
		public override bool EditableDuration { get { return false; } }

		public ButtonEncounterLogModel()
		{
			Buttons = new ListenerProperty<ButtonEdgeModel[]>(value => buttons = value, () => buttons);

			Style = new ListenerProperty<Styles>(value => style = value, () => style);

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