using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class TextHandlerModel : EncounterHandlerModel<TextEncounterLogModel>
	{
		[JsonProperty] string message;

		[JsonIgnore]
		public readonly ListenerProperty<string> Message;

		public TextHandlerModel()
		{
			Message = new ListenerProperty<string>(value => message = value, () => message);
		}
	}
}