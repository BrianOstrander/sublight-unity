using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class BustHandlerModel : EncounterHandlerModel<BustEncounterLogModel>
	{
		[JsonProperty] BustEntryModel[] entries;
		[JsonProperty] bool hasHaltingEvents;

		Action haltingDone;

		[JsonIgnore]
		public readonly ListenerProperty<BustEntryModel[]> Entries;
		[JsonIgnore]
		public readonly ListenerProperty<bool> HasHaltingEvents;
		[JsonIgnore]
		public readonly ListenerProperty<Action> HaltingDone;

		public BustHandlerModel(BustEncounterLogModel log) : base(log)
		{
			Entries = new ListenerProperty<BustEntryModel[]>(value => entries = value, () => entries);
			HasHaltingEvents = new ListenerProperty<bool>(value => hasHaltingEvents = value, () => hasHaltingEvents);
			HaltingDone = new ListenerProperty<Action>(value => haltingDone = value, () => haltingDone);
		}
	}
}