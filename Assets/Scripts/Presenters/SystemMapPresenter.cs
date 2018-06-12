using System;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SystemMapPresenter : Presenter<ISystemMapView>
	{
		SystemModel model;

		public SystemMapPresenter(SystemModel model)
		{
			this.model = model;
			SetView(App.V.Get<ISystemMapView>(v => v.SystemType == model.SystemType));

			App.Callbacks.StateChange += OnStateChange;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.StateChange -= OnStateChange;
		}

		public void Show(Action done = null)
		{
			if (View.Visible) return;
			View.Reset();
			View.UniversePosition = model.Position;
			View.Highlight = OnHighlight;
			View.Click = OnClick;
			if (done != null) View.Shown += done;
			ShowView(instant: true);
		}

		#region Events
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End) CloseView(true);
		}

		void OnHighlight(bool highlighted)
		{
			
		}

		void OnClick()
		{
			
		}
		#endregion
	}
}