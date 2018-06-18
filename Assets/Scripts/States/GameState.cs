using System;
using System.Linq;

using UnityEngine;

using LunraGames.SpaceFarm.Presenters;
using LunraGames.SpaceFarm.Models;
using LunraGames.NumberDemon;

namespace LunraGames.SpaceFarm
{
	public class GamePayload : IStatePayload
	{
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

		void InitializeGame(Action done)
		{
			//
			// --- Define Models --- 
			//
			Payload.Game = new GameModel();
			var game = Payload.Game;
			game.GameplayCanvas.Value = App.CanvasRoot;
			game.Seed.Value = DemonUtility.NextInteger;
			game.Universe.Value = App.UniverseService.CreateUniverse(1);
			game.FocusedSector.Value = new UniversePosition(Vector3.negativeInfinity, Vector3.negativeInfinity);
			game.FocusedSector.Changed += OnFocusedSector;
			game.DestructionSpeed.Value = 0.01f;

			var startSystem = game.Universe.Value.Sectors.Value.First().Systems.Value.First();
			var lastDistance = UniversePosition.Distance(UniversePosition.Zero, startSystem.Position);
			foreach (var system in game.Universe.Value.Sectors.Value.First().Systems.Value)
			{
				var distance = UniversePosition.Distance(UniversePosition.Zero, system.Position);
				if (lastDistance < distance) continue;
				lastDistance = distance;
				startSystem = system;
			}

			startSystem.Visited.Value = true;
			var startPosition = startSystem.Position;
			var rations = 0.3f;
			var speed = 0.003f;
			var rationConsumption = 0.02f;
			var travelRadiusChange = new TravelRadiusChange(startPosition, speed, rationConsumption, rations);

			var travelRequest = new TravelRequest(
				TravelRequest.States.Complete,
				startSystem.Position.Value,
				startSystem,
				startSystem,
				DayTime.Zero,
				DayTime.Zero,
				1f
			);

			var ship = new ShipModel();
			ship.CurrentSystem.Value = startSystem;
			ship.Position.Value = startPosition;
			ship.Speed.Value = travelRadiusChange.Speed;
			ship.RationConsumption.Value = travelRadiusChange.RationConsumption;
			ship.Rations.Value = travelRadiusChange.Rations;

			game.Ship.Value = ship;

			//
			// --- Initial Callbacks --- 
			//
			// We do this so basic important values are cached in the 
			// CallbackService.

			App.Callbacks.DayTimeDelta(new DayTimeDelta(DayTime.Zero, DayTime.Zero));
			App.Callbacks.SystemHighlight(SystemHighlight.None);
			App.Callbacks.TravelRadiusChange(travelRadiusChange);
			App.Callbacks.TravelRequest(travelRequest);
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
			new DestructionOriginSystemPresenter(game).Show();
			new DestructionSystemPresenter(game).Show();
			new DetailSystemPresenter(game);
			new LineSystemPresenter(game);
			new PauseMenuPresenter(game);
			new GameLostPresenter(game);
			new EnterSystemPresenter(game);

			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			Payload.Game.FocusedSector.Value = UniversePosition.Zero;

			App.Callbacks.SystemCameraRequest(SystemCameraRequest.RequestInstant(Payload.Game.Ship.Value.Position));
		}
		#endregion

		#region End
		protected override void End()
		{
			App.Input.SetEnabled(false);
			Payload.Game.FocusedSector.Changed -= OnFocusedSector;
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
		#endregion
	}
}