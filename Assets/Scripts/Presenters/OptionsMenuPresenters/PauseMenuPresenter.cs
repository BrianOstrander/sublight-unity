using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class PauseMenuPresenter : OptionsMenuPresenter
	{
		GameModel model;

		public PauseMenuPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.Escape += OnEscape;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.Escape -= OnEscape;
		}

		#region Events
		void OnEscape()
		{
			// Todo: some checking and stuff...
			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetPriorityFocus(model.ToolbarSelection.Value)));
			ShowView();
		}
		#endregion

	}
}