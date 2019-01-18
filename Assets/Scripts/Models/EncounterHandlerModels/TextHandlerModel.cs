using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class TextHandlerModel : EncounterHandlerModel<TextEncounterLogModel>
	{
		[JsonProperty] string message;

		[JsonIgnore]
		public readonly ListenerProperty<string> Message;

		public TextHandlerModel(TextEncounterLogModel log) : base(log)
		{
			Message = new ListenerProperty<string>(value => message = value, () => message);
		}
	}
}