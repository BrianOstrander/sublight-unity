using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class DialogEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<DialogEntryModel>
	{
		[JsonProperty] DialogEntryModel[] dialogs = new DialogEntryModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<DialogEntryModel[]> Dialogs;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Dialog; } }

		public override bool RequiresFallbackLog { get { return false; } }
		public override bool EditableDuration { get { return false; } }

		public DialogEncounterLogModel()
		{
			Dialogs = new ListenerProperty<DialogEntryModel[]>(value => dialogs = value, () => dialogs);
		}

		[JsonIgnore]
		public DialogEntryModel[] Edges
		{
			get { return Dialogs.Value; }
			set { Dialogs.Value = value; }
		}
	}
}