using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ConversationEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<ConversationEntryModel>
	{
		[JsonProperty] ConversationEntryModel[] entries = new ConversationEntryModel[0];
		[JsonIgnore] public readonly ListenerProperty<ConversationEntryModel[]> Entries;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Conversation; } }

		public override bool EditableDuration { get { return false; } }

		public ConversationEncounterLogModel()
		{
			Entries = new ListenerProperty<ConversationEntryModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public ConversationEntryModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}