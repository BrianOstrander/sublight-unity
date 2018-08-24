using System;
using System.Collections.Generic;

using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class ButtonEncounterLogPresenter : EntryEncounterLogPresenter<ButtonEncounterLogModel, IButtonEncounterLogView>
	{
		ButtonLogBlock[] buttons;

		public ButtonEncounterLogPresenter(GameModel model, ButtonEncounterLogModel logModel, ButtonLogBlock[] buttons) : base(model, logModel)
		{
			this.buttons = buttons;
		}

		protected override void OnShow()
		{
			var buttonList = new List<ButtonLogBlock>();
			foreach (var button in buttons) buttonList.Add(button.Duplicate(OnClick));
			View.Buttons = buttonList.ToArray();
		}

		#region Events
		void OnClick(Action done)
		{
			View.Interactable = false;
			done();
		}
		#endregion
	}
}