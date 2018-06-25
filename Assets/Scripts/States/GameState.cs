using System;

using UnityEngine;

using LunraGames.SpaceFarm.Presenters;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class GamePayload : IStatePayload
	{
		public GameSaveModel GameSave;
		public GameModel Game;
	}

	public class GameState : State<GamePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Game; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Game }; } }

		#region Begin
		protected override void Begin()
		{
			App.SM.PushBlocking(LoadScenes);
			App.SM.PushBlocking(InitializeCamera);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeCallbacks);
			App.SM.PushBlocking(InitializeGame);
		}

		void LoadScenes(Action done)
		{
			App.SceneService.Request(SceneRequest.Load(result => done(), Scenes));
		}

		void InitializeCamera(Action done)
		{
			new CameraSystemPresenter().Show(done);
		}

		void InitializeInput(Action done)
		{
			App.Input.SetEnabled(true);
			done();
		}

		void InitializeCallbacks(Action done)
		{
			App.Callbacks.SaveRequest += OnSaveRequest;
			App.Callbacks.TravelRequest += OnTravelRequest;
			done();
		}

		void InitializeGame(Action done)
		{
			//
			// --- Initial Callbacks --- 
			//
			// We do this so basic important values are cached in the 
			// CallbackService.

			var game = Payload.Game;

			game.FocusedSector.Changed += OnFocusedSector;

			var ship = game.Ship.Value;

			var travelRadiusChange = new TravelRadiusChange(ship.Position, ship.Speed, ship.RationConsumption, ship.Rations);

			App.Callbacks.DayTimeDelta(new DayTimeDelta(game.DayTime, game.DayTime));
			App.Callbacks.SystemHighlight(SystemHighlight.None);
			App.Callbacks.TravelRadiusChange(travelRadiusChange);
			App.Callbacks.TravelRequest(game.TravelRequest);
			App.Callbacks.SpeedRequest(SpeedRequest.PauseRequest);

			//
			// --- Create Presenters --- 
			//
			// There may be some warnings from creating these without assigning,
			// but you can safely ignore those warnings since they register
			// themselves with the PresenterMediator.

			new SpeedPresenter(game).Show();
			new ShipSystemPresenter(game).Show();
			new ShipRadiusPresenter(game).Show();
			new DestructionOriginSystemPresenter().Show();
			new DestructionSystemPresenter(game).Show();
			new DetailSystemPresenter(game);
			new LineSystemPresenter(game);
			new PauseMenuPresenter();
			new GameLostPresenter(game);
			new EnterSystemPresenter(game);

			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			var focusedSector = Payload.Game.Ship.Value.Position.Value.SystemZero;
			var wasFocused = Payload.Game.FocusedSector.Value == focusedSector;
			Payload.Game.FocusedSector.Value = focusedSector;
			if (wasFocused) OnFocusedSector(focusedSector);

			App.Callbacks.SystemCameraRequest(SystemCameraRequest.RequestInstant(Payload.Game.Ship.Value.Position));

			if (!DevPrefs.SkipExplanation)
			{
				App.Callbacks.DialogRequest(DialogRequest.Alert(Strings.Explanation, Strings.ExplanationTitle));
			}
		}
		#endregion

		#region End
		protected override void End()
		{
			App.Input.SetEnabled(false);

			App.Callbacks.SaveRequest -= OnSaveRequest;
			App.Callbacks.TravelRequest -= OnTravelRequest;
			Payload.Game.FocusedSector.Changed -= OnFocusedSector;
			App.Callbacks.ClearEscapables();

			App.SM.PushBlocking(UnBind);
			App.SM.PushBlocking(UnLoadScenes);
		}

		void UnBind(Action done)
		{
			// All presenters will have their views closed and unbinded. Events
			// will also be unbinded.
			App.P.UnRegisterAll(done);
		}

		void UnLoadScenes(Action done)
		{
			App.SceneService.Request(SceneRequest.UnLoad(result => done(), Scenes));
		}
		#endregion

		#region Events
		void OnFocusedSector(UniversePosition universePosition)
		{
			var sector = Payload.Game.Universe.Value.GetSector(universePosition);
			foreach (var system in sector.Systems.Value)
			{
				var systemPresenter = new SystemPresenter(Payload.Game, system);
				systemPresenter.Show();
			}
		}

		void OnTravelRequest(TravelRequest request)
		{
			Payload.Game.TravelRequest.Value = request;
		}

		void OnSaveRequest(SaveRequest request)
		{
			switch(request.State)
			{
				case SaveRequest.States.Request:
					App.SaveLoadService.Save(Payload.GameSave, OnSave);
					break;
			}
		}

		void OnSave(SaveLoadRequest<GameSaveModel> request)
		{
			if (request.Status != RequestStatus.Success) Debug.LogError("Error saving: " + request.Error);
			App.Callbacks.SaveRequest(new SaveRequest(SaveRequest.States.Complete));
		}
		#endregion
	}
}