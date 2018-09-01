using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class TextEncounterLogModel : LinearEncounterLogModel
	{
		[JsonProperty] LanguageStringModel _header = new LanguageStringModel();
		[JsonProperty] LanguageStringModel _message = new LanguageStringModel();

		[JsonProperty] string header;
		[JsonProperty] string message;

		[JsonIgnore]
		public readonly ListenerProperty<string> Header;
		[JsonIgnore]
		public readonly ListenerProperty<string> Message;

		[JsonIgnore]
		public LanguageStringModel _Header { get { return _header; } }
		[JsonIgnore]
		public LanguageStringModel _Message { get { return _message; } }

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Text; } }

		public TextEncounterLogModel()
		{
			Header = new ListenerProperty<string>(value => header = value, () => header);
			Message = new ListenerProperty<string>(value => message = value, () => message);
		}

		protected override void OnRegisterLanguageStrings()
		{
			AddLanguageStrings(_Header, _Message);
		}
	}
}