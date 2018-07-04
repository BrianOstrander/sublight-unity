using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class ProbeBodyPresenter : Presenter<IProbeBodyView>
	{
		GameModel model;
		SystemModel system;
		BodyModel body;

		public ProbeBodyPresenter(GameModel model)
		{
			this.model = model;

			App.Callbacks.FocusRequest += OnFocus;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.FocusRequest -= OnFocus;
		}

		public void Show()
		{
			if (View.Visible) return;

			View.Reset();

			View.BackClick = OnBackClick;

			ShowView(App.CanvasRoot);
		}

		#region Events
		void OnFocus(FocusRequest focus)
		{
			switch (focus.Focus)
			{
				case FocusRequest.Focuses.Body:
					// We only show UI elements once the focus is complete.
					if (focus.State != FocusRequest.States.Complete) return;
					var bodyFocus = focus as BodyFocusRequest;
					system = model.Universe.Value.GetSystem(bodyFocus.System);
					body = system.GetBody(bodyFocus.Body);
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
				new SystemBodiesFocusRequest(system.Position)
			);
		}
		#endregion

	}
}