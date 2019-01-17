using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class DialogHandlerModel : EncounterHandlerModel<DialogEncounterLogModel>
	{
		[JsonProperty] DialogLogBlock dialog;

		[JsonIgnore]
		public readonly ListenerProperty<DialogLogBlock> Dialog;

		public DialogHandlerModel()
		{
			Dialog = new ListenerProperty<DialogLogBlock>(value => dialog = value, () => dialog);
		}
	}
}