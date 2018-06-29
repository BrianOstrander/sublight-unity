using UnityEngine;

using LunraGames.SpaceFarm.Views;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm.Presenters
{
	public class GameLostPresenter : Presenter<IGameLostView>
	{
		GameModel model;
		bool hasPoppedEscape;

		public GameLostPresenter(GameModel model)
		{
			this.model = model;

			model.Ship.Value.Rations.Changed += OnRations;
			model.DestructionRadius.Changed += OnDestructionRadius;
			App.Callbacks.TravelRequest += OnTravelRequest;
		}

		protected override void UnBind()
		{
			base.UnBind();

			model.Ship.Value.Rations.Changed -= OnRations;
			model.DestructionRadius.Changed -= OnDestructionRadius;
			App.Callbacks.TravelRequest -= OnTravelRequest;
		}

		void Show(string reason)
		{
			if (View.Visible) return;

			App.Callbacks.ShadeRequest(ShadeRequest.Shade);
			App.Callbacks.ObscureCameraRequest(ObscureCameraRequest.Obscure);
			hasPoppedEscape = false;

			View.Reset();

			View.Shown += () => App.Callbacks.PushEscape(new EscapeEntry(OnEscape));

			View.Reason = reason;
			View.MainMenuClick = OnMainMenuClick;
			ShowView(App.OverlayCanvasRoot, true);
		}

		#region Events
		void OnEscape()
		{
			hasPoppedEscape = true;
			OnClose();
		}

		void OnRations(float rations)
		{
			if (!Mathf.Approximately(0f, rations)) return;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);
			Show(Strings.OutOfRations);
		}

		void OnTravelRequest(TravelRequest travelRequest)
		{
			switch (travelRequest.State)
			{
				case TravelRequest.States.Complete:
					// Don't pop up on end system.
					if (travelRequest.Destination == model.EndSystem.Value) return;
					var travelDestination = model.Universe.Value.GetSystem(travelRequest.Destination);
					var remainingFuel = (model.Ship.Value.Fuel - travelRequest.FuelConsumed) + travelDestination.Fuel;
					// Don't pop up if there's going to be enough fuel in this system.
					if (1f <= remainingFuel) return;
					Show(Strings.OutOfFuel);
					break;
			}
		}

		void OnDestructionRadius(float radius)
		{
			if (radius < UniversePosition.Distance(UniversePosition.Zero, model.Ship.Value.Position)) return;
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);
			Show(Strings.DestroyedByVoid);
		}

		void OnMainMenuClick()
		{
			switch(View.TransitionState)
			{
				case TransitionStates.Shown:
					OnClose();
					break;
			}
		}

		void OnClose()
		{
			if (!hasPoppedEscape)
			{
				App.Callbacks.PopEscape();
				App.Callbacks.ShadeRequest(ShadeRequest.UnShade);
				App.Callbacks.ObscureCameraRequest(ObscureCameraRequest.UnObscure);
			}
			View.Closed += OnClosed;
			CloseView();
		}

		void OnClosed()
		{
			var payload = new HomePayload();
			App.SM.RequestState(payload);
		}
		#endregion

	}
}