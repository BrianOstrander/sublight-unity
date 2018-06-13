using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using LunraGames.SpaceFarm.Presenters;
using LunraGames.SpaceFarm.Models;
using LunraGames.NumberDemon;

namespace LunraGames.SpaceFarm
{
	public class GamePayload : IStatePayload
	{
		public List<string> LoadedScenes = new List<string>();
		public List<string> CheckedTags = new List<string>();
		public List<string> FoundTags = new List<string>();

		public Action InitializeSceneCallback = ActionExtensions.Empty;
		public Action UnloadSceneCallback = ActionExtensions.Empty;

		public GameModel Game;
	}

	public class GameState : State<GamePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Game; } }

		static string[] Scenes
		{
			get
			{
				var scenes = new List<string>(new string[] {
					SceneConstants.Game
				});
				return scenes.ToArray();
			}
		}

		#region Begin
		protected override void Begin()
		{
			App.SM.PushBlocking(InitializeScenes);
			App.SM.PushBlocking(InitializeCamera);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeGame);
		}

		void InitializeScenes(Action callback)
		{
			Payload.InitializeSceneCallback = callback;
			App.Callbacks.SceneLoad += OnSceneInitialized;
			foreach (var scene in Scenes) SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		}

		void OnSceneInitialized(Scene scene, LoadSceneMode loadMode)
		{
			if (!Scenes.Contains(scene.name)) return;

			App.Log("Loaded Scene: " + scene.name, LogTypes.Initialization);

			Payload.LoadedScenes.Add(scene.name);
			SceneManager.SetActiveScene(scene);

			if (Scenes.Length != Payload.LoadedScenes.Count) return;

			App.Log("All Scenes Loaded", LogTypes.Initialization);

			var missingTags = Payload.CheckedTags.Where(t => !Payload.FoundTags.Contains(t));
			if (0 < missingTags.Count())
			{
				var tagWarning = "Missing Tags:";
				foreach (var tag in missingTags) tagWarning += "\n\t" + tag;
				Debug.LogError(tagWarning);
			}

			App.Callbacks.SceneLoad -= OnSceneInitialized;
			Payload.InitializeSceneCallback();
		}

		void InitializeCamera(Action done)
		{
			new ShipCameraPresenter().Show(done);
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

			var startSystem = game.Universe.Value.Sectors.Value.First().Systems.Value.First();
			var startPosition = startSystem.Position;
			var rations = 0.25f;
			var speed = 0.001f;
			var rationConsumption = 0.02f;
			var travelRadiusChange = new TravelRadiusChange(startPosition, speed, rationConsumption, rations);

			var travelProgress = new TravelProgress(
				TravelProgress.States.Complete,
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

			App.Callbacks.TravelRadiusChange(travelRadiusChange);
			App.Callbacks.TravelProgress(travelProgress);

			//
			// --- Create Presenters --- 
			//
			// There may be some warnings from creating these without assigning,
			// but you can safely ignore those warnings since they register
			// themselves with the PresenterMediator.

			new SpeedPresenter(game).Show();
			new ShipMapPresenter(game).Show();
			new ShipRadiusPresenter(game).Show();
			new SystemDetailPresenter(game);
			new SystemLinePresenter(game);
			new PauseMenuPresenter(game);

			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			Payload.Game.FocusedSector.Value = UniversePosition.Zero;
		}
		#endregion

		#region End
		protected override void End()
		{
			App.Input.SetEnabled(false);
			Payload.Game.FocusedSector.Changed -= OnFocusedSector;
			App.SM.PushBlocking(UnBind);
			App.SM.PushBlocking(UnloadScene);
		}

		void UnBind(Action done)
		{
			// All presenters will have their views closed and unbinded. Events
			// will also be unbinded.
			App.P.UnRegisterAll(done);
		}
		
		void UnloadScene(Action done)
		{
			Payload.UnloadSceneCallback = done;
			App.Callbacks.SceneUnload += OnSceneUnloaded;
			SceneManager.UnloadSceneAsync(SceneConstants.Game);
		}

		void OnSceneUnloaded(Scene scene)
		{
			App.Callbacks.SceneUnload -= OnSceneUnloaded;
			Payload.UnloadSceneCallback();
		}
		#endregion

		#region Events
		void OnFocusedSector(UniversePosition universePosition)
		{
			var sector = Payload.Game.Universe.Value.GetSector(universePosition);
			foreach (var system in sector.Systems.Value)
			{
				var systemPresenter = new SystemMapPresenter(Payload.Game, system);
				systemPresenter.Show();
			}
		}

		void OnRations(float rations)
		{
			
		}
		#endregion
	}
}