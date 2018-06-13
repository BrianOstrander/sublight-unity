using System;

using UnityEngine;

using LunraGames.SpaceFarm.Models;
using LunraGames.SpaceFarm.Views;

namespace LunraGames.SpaceFarm.Presenters
{
	public class SystemLinePresenter : Presenter<ISystemLineView>
	{
		SystemHighlight nextHighlight;

		GameModel gameModel;

		public SystemLinePresenter(GameModel gameModel)
		{
			this.gameModel = gameModel;

			App.Callbacks.StateChange += OnStateChange;
			App.Callbacks.SystemHighlight += OnSystemHighlight;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.StateChange -= OnStateChange;
			App.Callbacks.SystemHighlight -= OnSystemHighlight;
		}

		public void Show()
		{
			if (View.Visible) return;
			View.Reset();
			View.Closed += OnClose;
			ShowView(instant: true);
		}

		#region Events
		void OnStateChange(StateChange state)
		{
			if (state.Event == StateMachine.Events.End)
			{
				nextHighlight = SystemHighlight.None;
				CloseView(true);
			}
		}

		void OnDetails(SystemModel origin, SystemModel destination, TravelRadius travelRadius)
		{
			var distance = UniversePosition.Distance(origin.Position, destination.Position);

			var safeStart = 0f;
			var safeEnd = Mathf.Min(distance, travelRadius.SafeRadius);
			var dangerStart = safeEnd;
			var dangerEnd = Mathf.Min(distance, travelRadius.DangerRadius);
			var maxStart = dangerEnd;
			var maxEnd = Mathf.Min(distance, travelRadius.MaximumRadius);



			//var normal = total.normalized;

		}

		void OnTravelRadiusChange(TravelRadiusChange travelRadiusChange)
		{
			
		}

		void OnSystemHighlight(SystemHighlight highlight)
		{
			nextHighlight = highlight;
			switch (highlight.State)
			{
				case SystemHighlight.States.End:
				case SystemHighlight.States.Change:
					if (View.TransitionState == TransitionStates.Shown) CloseView(true);
					break;
				case SystemHighlight.States.Begin:
					//Show( highlight.System);
					break;
			}
		}

		void OnClose()
		{
			switch (nextHighlight.State)
			{
				case SystemHighlight.States.Begin:
				case SystemHighlight.States.Change:
					//Show(nextHighlight.System);
					break;
			}
		}
		#endregion
	}
}