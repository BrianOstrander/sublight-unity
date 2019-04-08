using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ConversationHandlerModel : EncounterHandlerModel<ConversationEncounterLogModel>
	{
		[JsonProperty] ConversationEntryModel[] entries;
		[JsonIgnore] public readonly ListenerProperty<ConversationEntryModel[]> Entries;

		Action haltingDone;
		[JsonIgnore] public readonly ListenerProperty<Action> HaltingDone;

		public ConversationHandlerModel(ConversationEncounterLogModel log) : base(log)
		{
			Entries = new ListenerProperty<ConversationEntryModel[]>(value => entries = value, () => entries);
			HaltingDone = new ListenerProperty<Action>(value => haltingDone = value, () => haltingDone);
		}
	}
}