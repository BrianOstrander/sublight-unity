﻿using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ButtonHandlerModel : EncounterHandlerModel<ButtonEncounterLogModel>
	{
		ButtonLogBlock[] buttons = new ButtonLogBlock[0];
		[JsonIgnore] public readonly ListenerProperty<ButtonLogBlock[]> Buttons;

		public ButtonHandlerModel(ButtonEncounterLogModel log) : base(log)
		{
			Buttons = new ListenerProperty<ButtonLogBlock[]>(value => buttons = value, () => buttons);
		}
	}
}