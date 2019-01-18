using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEncounterLogModel : LinearEncounterLogModel, IEdgedEncounterLogModel<ButtonEdgeModel>
	{
		[JsonProperty] ButtonEdgeModel[] buttons = new ButtonEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<ButtonEdgeModel[]> Buttons;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Button; } }

		public override bool RequiresNextLog { get { return false; } }
		public override bool EditableDuration { get { return false; } }

		public ButtonEncounterLogModel()
		{
			Buttons = new ListenerProperty<ButtonEdgeModel[]>(value => buttons = value, () => buttons);
		}

		[JsonIgnore]
		public ButtonEdgeModel[] Edges
		{
			get { return Buttons.Value; }
			set { Buttons.Value = value; }
		}
	}
}