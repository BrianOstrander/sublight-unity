using System;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonEncounterLogModel : EncounterLogModel, IEdgedEncounterLogModel<ButtonEntryModel>
	{
		[JsonProperty] ButtonEntryModel[] buttons = new ButtonEntryModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<ButtonEntryModel[]> Buttons;

		public override EncounterLogTypes LogType { get { return EncounterLogTypes.Button; } }

		public override bool RequiresFallbackLog { get { return false; } }
		public override bool EditableDuration { get { return false; } }

		public ButtonEncounterLogModel()
		{
			Buttons = new ListenerProperty<ButtonEntryModel[]>(value => buttons = value, () => buttons);
		}

		[JsonIgnore]
		public ButtonEntryModel[] Edges
		{
			get { return Buttons.Value; }
			set { Buttons.Value = value; }
		}
	}
}