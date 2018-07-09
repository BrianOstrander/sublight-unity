using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class TextEncounterLogModel : EncounterLogModel
	{
		[JsonProperty] string text;

		[JsonIgnore]
		public readonly ListenerProperty<string> Text;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Text; } }

		public TextEncounterLogModel()
		{
			Text = new ListenerProperty<string>(value => text = value, () => text);
		}
	}
}