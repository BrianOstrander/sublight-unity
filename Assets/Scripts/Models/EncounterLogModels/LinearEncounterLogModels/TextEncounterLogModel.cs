using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class TextEncounterLogModel : LinearEncounterLogModel
	{
		[JsonProperty] string header;
		[JsonProperty] string message;

		[JsonIgnore]
		public readonly ListenerProperty<string> Header;
		[JsonIgnore]
		public readonly ListenerProperty<string> Message;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Text; } }

		public TextEncounterLogModel()
		{
			Header = new ListenerProperty<string>(value => header = value, () => header);
			Message = new ListenerProperty<string>(value => message = value, () => message);
		}
	}
}