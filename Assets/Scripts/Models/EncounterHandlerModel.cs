using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public interface IEncounterHandlerModel : IModel {}

	public abstract class EncounterHandlerModel<T> : Model, IEncounterHandlerModel 
		where T : EncounterLogModel
	{
		[JsonProperty] T log;

		[JsonIgnore]
		public readonly ListenerProperty<T> Log;

		public EncounterHandlerModel()
		{
			Log = new ListenerProperty<T>(value => log = value, () => log);
		}
	}
}