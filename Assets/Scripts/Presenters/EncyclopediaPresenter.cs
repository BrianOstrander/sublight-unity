using LunraGames.SubLight.Views;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight.Presenters
{
	public class EncyclopediaPresenter : Presenter<IEncyclopediaView>
	{
		GameModel model;

		public EncyclopediaPresenter(GameModel model)
		{
			this.model = model;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.Title = "Select an Article";

			View.BackClick = OnBackClick;

			ShowView(App.GameCanvasRoot);
		}

		#region Events
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Encyclopedia:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var encyclopediaFocus = focus as EncyclopediaFocusRequest;
					// We also only show up if our view is specified
					if (encyclopediaFocus.View != EncyclopediaFocusRequest.Views.Home) goto default;
					Show();
					break;
				default:
					if (View.TransitionState == TransitionStates.Shown) CloseView();
					break;
			}
		}

		void OnBackClick()
		{
			if (View.TransitionState != TransitionStates.Shown) return;

			App.Callbacks.FocusRequest(
				new SystemsFocusRequest(
					model.Ship.Value.Position.Value.SystemZero,
					model.Ship.Value.Position.Value
				)
			);
		}
		#endregion
	}
}