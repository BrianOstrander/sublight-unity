using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ConversationHandlerModel : EncounterHandlerModel<ConversationEncounterLogModel>
	{
		[JsonProperty] ConversationEdgeModel[] entries;
		[JsonIgnore] public readonly ListenerProperty<ConversationEdgeModel[]> Entries;

		Action haltingDone;
		[JsonIgnore] public readonly ListenerProperty<Action> HaltingDone;

		public ConversationHandlerModel(ConversationEncounterLogModel log) : base(log)
		{
			Entries = new ListenerProperty<ConversationEdgeModel[]>(value => entries = value, () => entries);
			HaltingDone = new ListenerProperty<Action>(value => haltingDone = value, () => haltingDone);
		}
	}
}