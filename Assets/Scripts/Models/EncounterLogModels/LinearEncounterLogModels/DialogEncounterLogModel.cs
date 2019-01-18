using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class DialogEncounterLogModel : LinearEncounterLogModel, IEdgedEncounterLogModel<DialogEdgeModel>
	{
		[JsonProperty] DialogEdgeModel[] dialogs = new DialogEdgeModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<DialogEdgeModel[]> Dialogs;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Dialog; } }

		public override bool RequiresNextLog { get { return false; } }
		public override bool EditableDuration { get { return false; } }

		public DialogEncounterLogModel()
		{
			Dialogs = new ListenerProperty<DialogEdgeModel[]>(value => dialogs = value, () => dialogs);
		}

		[JsonIgnore]
		public DialogEdgeModel[] Edges
		{
			get { return Dialogs.Value; }
			set { Dialogs.Value = value; }
		}
	}
}