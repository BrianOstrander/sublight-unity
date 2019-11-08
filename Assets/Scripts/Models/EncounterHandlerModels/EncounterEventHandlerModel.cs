using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class EncounterEventHandlerModel : EncounterHandlerModel<EncounterEventEncounterLogModel>
	{
		[JsonProperty] EncounterEventEdgeModel[] events;
		[JsonProperty] bool alwaysHalting;
		[JsonProperty] bool hasHaltingEvents;

		Action haltingDone;

		[JsonIgnore]
		public readonly ListenerProperty<EncounterEventEdgeModel[]> Events;
		/// <summary>
		/// True if the entire event was marked as halting or any of its entries.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> AlwaysHalting;
		[JsonIgnore]
		public readonly ListenerProperty<bool> HasHaltingEvents;
		[JsonIgnore]
		public readonly ListenerProperty<Action> HaltingDone;

		public EncounterEventHandlerModel(EncounterEventEncounterLogModel log) : base(log)
		{
			Events = new ListenerProperty<EncounterEventEdgeModel[]>(value => events = value, () => events);
			AlwaysHalting = new ListenerProperty<bool>(value => alwaysHalting = value, () => alwaysHalting);
			HasHaltingEvents = new ListenerProperty<bool>(value => hasHaltingEvents = value, () => hasHaltingEvents);
			HaltingDone = new ListenerProperty<Action>(value => haltingDone = value, () => haltingDone);
		}
	}
}