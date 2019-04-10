using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ConversationEdgeModel : EdgeModel
	{
		[JsonProperty] ConversationEntryModel entry = new ConversationEntryModel();

		[JsonIgnore]
		public ConversationEntryModel Entry { get { return entry; } }

		public override EdgeEntryModel RawEntry { get { return Entry; } }
		public override string EdgeName
		{
			get
			{
				return Entry.ConversationType.Value.ToString();
			}
		}

		public override float EdgeIndent
		{
			get
			{
				switch (entry.ConversationType.Value)
				{
					case ConversationTypes.MessageOutgoing:
					case ConversationTypes.Prompt:
						return DefaultIndent;
				}
				return 0f;
			}
		}
	}
}