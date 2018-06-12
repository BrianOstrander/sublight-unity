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
		//public float SomeVariable;
	}

	public class GameState : State<GamePayload>
	{
		public override StateMachine.States HandledState { get { return StateMachine.States.Game; } }

		static string[] Scenes
		{
			get
			{
				var scenes = new List<string>(new string[] {
					"Game"
				});
				return scenes.ToArray();
			}
		}

		List<string> loadedScenes = new List<string>();
		List<string> checkedTags = new List<string>();
		List<string> foundTags = new List<string>();
		Action initializeSceneCallback = ActionExtensions.Empty;

		GameModel game;

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
			initializeSceneCallback = callback;
			App.Callbacks.SceneLoad += OnSceneInitialized;
			foreach (var scene in Scenes) SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		}

		void OnSceneInitialized(Scene scene, LoadSceneMode loadMode)
		{
			if (!Scenes.Contains(scene.name)) return;

			App.Log("Loaded Scene: " + scene.name, LogTypes.Initialization);

			loadedScenes.Add(scene.name);
			SceneManager.SetActiveScene(scene);

			if (Scenes.Length != loadedScenes.Count) return;

			App.Log("All Scenes Loaded", LogTypes.Initialization);

			var missingTags = checkedTags.Where(t => !foundTags.Contains(t));
			if (0 < missingTags.Count())
			{
				var tagWarning = "Missing Tags:";
				foreach (var tag in missingTags) tagWarning += "\n\t" + tag;
				Debug.LogError(tagWarning);
			}

			App.Callbacks.SceneLoad -= OnSceneInitialized;
			initializeSceneCallback();
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
			game = new GameModel();
			game.GameplayCanvas.Value = App.CanvasRoot;
			game.Seed.Value = DemonUtility.NextInteger;
			game.Universe.Value = App.UniverseService.CreateUniverse(1);
			game.FocusedSector.Value = new UniversePosition(Vector3.negativeInfinity, Vector3.negativeInfinity);
			game.FocusedSector.Changed += OnFocusedSector;

			var startSystem = game.Universe.Value.Sectors.Value.First().Systems.Value.First();
			var startPosition = startSystem.Position;
			var rations = 1f;
			var speed = 0.001f;
			var rationConsumption = 0.02f;
			var travelRadiusChange = new TravelRadiusChange(startPosition, speed, rationConsumption, rations);

			App.Callbacks.TravelRadiusChange(travelRadiusChange);

			var ship = new ShipModel();
			ship.CurrentSystem.Value = startSystem;
			ship.Position.Value = startPosition;
			ship.Speed.Value = travelRadiusChange.Speed;
			ship.RationConsumption.Value = travelRadiusChange.RationConsumption;
			ship.Rations.Value = travelRadiusChange.Rations;

			game.Ship.Value = ship;

			// TODO: Figure out where to assign these.
			new SpeedPresenter(game).Show();
			new ShipMapPresenter(game).Show();
			new ShipRadiusPresenter(game).Show();
			new SystemDetailPresenter(game);

			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			game.FocusedSector.Value = UniversePosition.Zero;
		}
		#endregion

		#region End
		protected override void End()
		{
			game.FocusedSector.Changed -= OnFocusedSector;
		}
		#endregion

		#region Events
		void OnFocusedSector(UniversePosition universePosition)
		{
			var sector = game.Universe.Value.GetSector(universePosition);
			foreach (var system in sector.Systems.Value)
			{
				var systemPresenter = new SystemMapPresenter(game, system);
				systemPresenter.Show();
			}
		}
		#endregion
	}
}