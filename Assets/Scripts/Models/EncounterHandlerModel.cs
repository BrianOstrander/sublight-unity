using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public interface IEncounterHandlerModel : IModel
	{
		EncounterLogTypes LogType { get; }
	}

	/// <summary>
	/// Handler models are used by the encounter service when a halting encounter log requires
	/// a presenter or some other runtime binded functionality to process encounter logic.
	/// For example, a view may be launched that requires user input before the encounter can continue. 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class EncounterHandlerModel<T> : Model, IEncounterHandlerModel 
		where T : EncounterLogModel
	{
		[JsonProperty] T log;

		[JsonIgnore]
		public readonly ListenerProperty<T> Log;

		public EncounterLogTypes LogType => Log.Value?.LogType ?? EncounterLogTypes.Unknown;

		public EncounterHandlerModel(
			T log
		)
		{
			this.log = log;

			Log = new ListenerProperty<T>(value => this.log = value, () => this.log);
		}
	}
}