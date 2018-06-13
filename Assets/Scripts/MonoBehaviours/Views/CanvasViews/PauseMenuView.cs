using System;

namespace LunraGames.SpaceFarm.Views
{
	public class PauseMenuView : CanvasView, IPauseMenuView
	{
		public Action BackClick { set; private get; }
		public Action MainMenuClick { set; private get; }

		public override void Reset()
		{
			base.Reset();

			BackClick = ActionExtensions.Empty;
			MainMenuClick = ActionExtensions.Empty;
		}

		#region Events
		public void OnBackClick() { BackClick(); }
		public void OnMainMenuClick() { MainMenuClick(); }
		#endregion
	}

	public interface IPauseMenuView : ICanvasView
	{
		Action BackClick { set; }
		Action MainMenuClick { set; }
	}
}