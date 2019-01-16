using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public interface IEncounterHandlerModel : IModel {}

	public abstract class EncounterHandlerModel<T> : Model, IEncounterHandlerModel 
		where T : EncounterLogModel
	{
		[JsonProperty] T log;
		[JsonProperty] bool isHalting;

		Action haltingDone;

		[JsonIgnore]
		public readonly ListenerProperty<T> Log;
		[JsonIgnore]
		public readonly ListenerProperty<bool> IsHalting;
		[JsonIgnore]
		public readonly ListenerProperty<Action> HaltingDone;

		public EncounterHandlerModel()
		{
			Log = new ListenerProperty<T>(value => log = value, () => log);
			IsHalting = new ListenerProperty<bool>(value => isHalting = value, () => isHalting);
			HaltingDone = new ListenerProperty<Action>(value => haltingDone = value, () => haltingDone);
		}
	}
}