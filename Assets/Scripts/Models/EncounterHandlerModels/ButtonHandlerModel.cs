using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonHandlerModel : EncounterHandlerModel<ButtonEncounterLogModel>
	{
		[JsonProperty] ButtonLogBlock[] buttons = new ButtonLogBlock[0];

		[JsonIgnore]
		public readonly ListenerProperty<ButtonLogBlock[]> Buttons;

		public ButtonHandlerModel()
		{
			Buttons = new ListenerProperty<ButtonLogBlock[]>(value => buttons = value, () => buttons);
		}
	}
}