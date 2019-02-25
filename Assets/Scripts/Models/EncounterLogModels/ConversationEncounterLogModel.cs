using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ConversationEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<ConversationEdgeModel>
	{
		[JsonProperty] ConversationEdgeModel[] entries = new ConversationEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<ConversationEdgeModel[]> Entries;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Conversation; } }

		public override bool EditableDuration { get { return false; } }

		public ConversationEncounterLogModel()
		{
			Entries = new ListenerProperty<ConversationEdgeModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public ConversationEdgeModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}