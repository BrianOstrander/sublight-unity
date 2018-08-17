using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class CameraShipPresenter : Presenter<ICameraShipView>
	{
		public CameraShipPresenter()
		{
			App.Callbacks.FocusRequest += OnFocus;
		}

		protected override void OnUnBind()
		{
			App.Callbacks.FocusRequest -= OnFocus;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			ShowView(instant: true);
		}

		#region Events
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Ship:
					if (focus.State == FocusRequest.States.Active) Show();
					break;
				default:
					if (focus.State == FocusRequest.States.Active)
					{
						if (View.TransitionState == TransitionStates.Shown) CloseView();
					}
					break;
			}
		}
		#endregion
	}
}