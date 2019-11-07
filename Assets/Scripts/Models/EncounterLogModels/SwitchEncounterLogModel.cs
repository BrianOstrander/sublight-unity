using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SwitchEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<SwitchEntryModel>
	{
		public enum SelectionMethods
		{
			Unknown = 0,
			FirstFilter = 10,
			Random = 20,
			RandomWeighted = 30
		}

		[JsonProperty] SelectionMethods selectionMethod;
		[JsonIgnore] public readonly ListenerProperty<SelectionMethods> SelectionMethod;

		[JsonProperty] SwitchEntryModel[] switches = new SwitchEntryModel[0];
		[JsonIgnore] public readonly ListenerProperty<SwitchEntryModel[]> Switches;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Switch; } }

		public override bool RequiresFallbackLog { get { return false; } }
		public override bool EditableDuration { get { return false; } }

		public SwitchEncounterLogModel()
		{
			SelectionMethod = new ListenerProperty<SelectionMethods>(value => selectionMethod = value, () => selectionMethod);
			Switches = new ListenerProperty<SwitchEntryModel[]>(value => switches = value, () => switches);
		}

		[JsonIgnore]
		public SwitchEntryModel[] Edges
		{
			get { return Switches.Value; }
			set { Switches.Value = value; }
		}
	}
}