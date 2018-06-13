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
			OnDetails();
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

		void OnDetails()
		{
			var origin = gameModel.Ship.Value.CurrentSystem.Value;
			var destination = nextHighlight.System;
			var travelRadius = App.Callbacks.LastTravelRadiusChange.TravelRadius;

			var distance = UniversePosition.Distance(origin.Position, destination.Position);

			var safeEnd = Mathf.Min(distance, travelRadius.SafeRadius);
			var dangerEnd = Mathf.Min(distance, travelRadius.DangerRadius);
			var maxEnd = Mathf.Min(distance, travelRadius.MaximumRadius);
			var remainderEnd = distance;

			var unityOrigin = UniversePosition.ToUnity(origin.Position);

			var normal = (UniversePosition.ToUnity(destination.Position) - unityOrigin).normalized;

			LineSegment? safeSegment = null;
			LineSegment? dangerSegment = null;
			LineSegment? maxSegment = null;
			LineSegment? remainderSegment = null;

			if (0f < safeEnd)
			{
				safeSegment = new LineSegment(origin.Position, origin.Position + new UniversePosition(normal * safeEnd));
			}
			if (safeEnd < dangerEnd)
			{
				dangerSegment = new LineSegment(origin.Position + new UniversePosition(normal * safeEnd), origin.Position + new UniversePosition(normal * dangerEnd));
			}
			if (dangerEnd < maxEnd)
			{
				maxSegment = new LineSegment(origin.Position + new UniversePosition(normal * dangerEnd), origin.Position + new UniversePosition(normal * maxEnd));
			}
			if (maxEnd < remainderEnd)
			{
				remainderSegment= new LineSegment(origin.Position + new UniversePosition(normal * maxEnd), origin.Position + new UniversePosition(normal * remainderEnd));
			}

			View.SetSegments(safeSegment, dangerSegment, maxSegment, remainderSegment);

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
					Show();
					break;
			}
		}

		void OnClose()
		{
			switch (nextHighlight.State)
			{
				case SystemHighlight.States.Begin:
				case SystemHighlight.States.Change:
					Show();
					break;
			}
		}
		#endregion
	}
}