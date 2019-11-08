using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	// What a name...
	public class EncounterEventEncounterLogModel : EdgedEncounterLogModel<EncounterEventEdgeModel>
	{
		[JsonProperty] bool alwaysHalting;
		[JsonIgnore] public readonly ListenerProperty<bool> AlwaysHalting;

		public override EncounterLogTypes LogType => EncounterLogTypes.Event;
		public override bool EditableDuration => false;

		public EncounterEventEncounterLogModel()
		{
			AlwaysHalting = new ListenerProperty<bool>(value => alwaysHalting = value, () => alwaysHalting);
		}
	}
}