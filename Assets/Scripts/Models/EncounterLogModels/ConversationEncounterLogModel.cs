using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ConversationEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<ConversationEdgeModel>
	{
		[JsonProperty] ConversationEdgeModel[] entries = new ConversationEdgeModel[0];
		[JsonIgnore] public readonly ListenerProperty<ConversationEdgeModel[]> Entries;

		[JsonProperty] ConversationButtonStyles style;
		[JsonIgnore] public readonly ListenerProperty<ConversationButtonStyles> Style;

		[JsonProperty] ConversationThemes theme;
		[JsonIgnore] public readonly ListenerProperty<ConversationThemes> Theme;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Conversation; } }

		public override bool EditableDuration { get { return false; } }

		public ConversationEncounterLogModel()
		{
			Entries = new ListenerProperty<ConversationEdgeModel[]>(value => entries = value, () => entries);
			Style = new ListenerProperty<ConversationButtonStyles>(value => style = value, () => style);
			Theme = new ListenerProperty<ConversationThemes>(value => theme = value, () => theme);
		}

		[JsonIgnore]
		public ConversationEdgeModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}