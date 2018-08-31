using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class TextEncounterLogModel : LinearEncounterLogModel
	{
		[JsonProperty] string header;
		[JsonProperty] LanguageStringModel _header = new LanguageStringModel();
		[JsonProperty] string message;

		[JsonIgnore]
		public readonly ListenerProperty<string> Header;
		[JsonIgnore]
		public readonly ListenerProperty<string> Message;

		[JsonIgnore]
		public LanguageStringModel _Header { get { return _header; } }

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Text; } }

		public TextEncounterLogModel()
		{
			Header = new ListenerProperty<string>(value => header = value, () => header);
			Message = new ListenerProperty<string>(value => message = value, () => message);

			AddLanguageStrings(_Header);
		}
	}
}