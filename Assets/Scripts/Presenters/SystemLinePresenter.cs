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

			App.Callbacks.TravelRadiusChange += OnTravelRadiusChange;
			App.Callbacks.SystemHighlight += OnSystemHighlight;
			App.Callbacks.TravelProgress += OnTravelProgress;
		}

		protected override void UnBind()
		{
			base.UnBind();

			App.Callbacks.TravelRadiusChange -= OnTravelRadiusChange;
			App.Callbacks.SystemHighlight -= OnSystemHighlight;
			App.Callbacks.TravelProgress -= OnTravelProgress;
		}

		public void Show()
		{
			if (View.Visible || App.Callbacks.LastTravelProgress.State != TravelProgress.States.Complete) return;
			View.Reset();
			OnDetails();
			View.Closed += OnClosed;
			ShowView(instant: true);
		}

		#region Events
		void OnTravelRadiusChange(TravelRadiusChange travelRadiusChange) 
		{
			if (!View.Visible) return;
			OnDetails(); 
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

		void OnTravelProgress(TravelProgress travelProgress)
		{
			if (View.Visible && travelProgress.State == TravelProgress.States.Request) CloseView(true);
		}

		void OnClosed()
		{
			if (App.SM.CurrentEvent == StateMachine.Events.End) return;

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