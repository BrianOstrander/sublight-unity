using System;

using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class HomeMenuPresenter : Presenter<IHomeMenuView>
	{
		public HomeMenuPresenter()
		{
			App.Callbacks.StateChange += OnStateChange;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.StateChange -= OnStateChange;
		}

		public void Show(Action done)
		{
			if (View.Visible) return;
			View.Reset();
			View.StartClick = OnStartClick;
			View.Shown += done;
			ShowView(App.CanvasRoot, true);
		}

		#region Events
		void OnStartClick()
		{
			var payload = new GamePayload();
			App.SM.RequestState(payload);
		}

		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End) CloseView();
		}
		#endregion

	}
}