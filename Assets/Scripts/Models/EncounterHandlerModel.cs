using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public interface IEncounterHandlerModel : IModel
	{
		EncounterLogTypes LogType { get; }
	}

	public abstract class EncounterHandlerModel<T> : Model, IEncounterHandlerModel 
		where T : EncounterLogModel
	{
		[JsonProperty] T log;

		[JsonIgnore]
		public readonly ListenerProperty<T> Log;

		public EncounterLogTypes LogType { get { return Log.Value == null ? EncounterLogTypes.Unknown : Log.Value.LogType; } }

		public EncounterHandlerModel(
			T log
		)
		{
			this.log = log;

			Log = new ListenerProperty<T>(value => this.log = value, () => this.log);
		}
	}
}