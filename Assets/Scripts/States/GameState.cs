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

			// TODO: Figure out where to assign these.
			var speed = new SpeedPresenter(game);
			speed.Show();

			done();
		}
		#endregion

		#region Idle
		#endregion
	}
}