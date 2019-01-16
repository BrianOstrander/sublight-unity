using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public interface IEncounterHandlerModel : IModel {}

	public abstract class EncounterHandlerModel<T> : Model, IEncounterHandlerModel 
		where T : EncounterLogModel
	{
		[JsonProperty] T log;
		[JsonProperty] bool alwaysHalting;
		[JsonProperty] bool hasHaltingEvents;

		Action haltingDone;

		[JsonIgnore]
		public readonly ListenerProperty<T> Log;
		/// <summary>
		/// True if the entire event was marked as halting or any of its entries.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<bool> AlwaysHalting;
		[JsonIgnore]
		public readonly ListenerProperty<bool> HasHaltingEvents;
		[JsonIgnore]
		public readonly ListenerProperty<Action> HaltingDone;

		[JsonIgnore]
		public bool AlwaysHaltingOrHasHaltingEvents { get { return AlwaysHalting.Value || HasHaltingEvents.Value; } }

		public EncounterHandlerModel()
		{
			Log = new ListenerProperty<T>(value => log = value, () => log);
			AlwaysHalting = new ListenerProperty<bool>(value => alwaysHalting = value, () => alwaysHalting);
			HasHaltingEvents = new ListenerProperty<bool>(value => hasHaltingEvents = value, () => hasHaltingEvents);
			HaltingDone = new ListenerProperty<Action>(value => haltingDone = value, () => haltingDone);
		}
	}
}