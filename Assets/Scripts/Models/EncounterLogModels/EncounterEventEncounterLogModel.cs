using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	// What a name...
	public class EncounterEventEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<EncounterEventEntryModel>
	{
		[JsonProperty] bool alwaysHalting;
		[JsonProperty] EncounterEventEntryModel[] entries = new EncounterEventEntryModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<bool> AlwaysHalting;
		[JsonIgnore]
		public readonly ListenerProperty<EncounterEventEntryModel[]> Entries;

		public override EncounterLogTypes LogType => EncounterLogTypes.Event;

		public override bool EditableDuration => false;

		public EncounterEventEncounterLogModel()
		{
			AlwaysHalting = new ListenerProperty<bool>(value => alwaysHalting = value, () => alwaysHalting);
			Entries = new ListenerProperty<EncounterEventEntryModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public EncounterEventEntryModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
	}
}