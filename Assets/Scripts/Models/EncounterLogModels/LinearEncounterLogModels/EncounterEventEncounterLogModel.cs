using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	// What a name...
	public class EncounterEventEncounterLogModel : LinearEncounterLogModel, IEdgedEncounterLogModel<EncounterEventEdgeModel>
	{
		[JsonProperty] bool alwaysHalting;
		[JsonProperty] EncounterEventEdgeModel[] entries = new EncounterEventEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<bool> AlwaysHalting;
		[JsonIgnore]
		public readonly ListenerProperty<EncounterEventEdgeModel[]> Entries;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Event; } }

		public override bool EditableDuration { get { return false; } }

		public EncounterEventEncounterLogModel()
		{
			AlwaysHalting = new ListenerProperty<bool>(value => alwaysHalting = value, () => alwaysHalting);
			Entries = new ListenerProperty<EncounterEventEdgeModel[]>(value => entries = value, () => entries);
		}

		[JsonIgnore]
		public EncounterEventEdgeModel[] Edges
		{
			get { return Entries.Value; }
			set { Entries.Value = value; }
		}
		[JsonIgnore]
		public bool IsLinear { get { return true; } }
	}
}