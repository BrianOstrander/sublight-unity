using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using LunraGames.SpaceFarm.Presenters;

namespace LunraGames.SpaceFarm
{
	public class HomePayload : IStatePayload 
	{
		public List<string> LoadedScenes = new List<string>();
		public List<string> CheckedTags = new List<string>();
		public List<string> FoundTags = new List<string>();
		public Action InitializeSceneCallback = ActionExtensions.Empty;

		public Action UnloadSceneCallback = ActionExtensions.Empty;

	}

	public class HomeState : State<HomePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Home; } }

		static string[] Scenes
		{
			get
			{
				var scenes = new List<string>(new string[] {
					SceneConstants.Home
				});
				return scenes.ToArray();
			}
		}

		#region Begin
		protected override void Begin()
		{
			App.SM.PushBlocking(InitializeScenes);
		}

		void InitializeScenes(Action callback)
		{
			App.Log("Loading Scenes", LogTypes.Initialization);
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

		void AssignTag(ref Transform existing, string tag)
		{
			if (existing != null) return;

			if (!Payload.CheckedTags.Contains(tag)) Payload.CheckedTags.Add(tag);
			var tagObject = GameObject.FindWithTag(tag);

			if (tagObject == null) return;

			existing = tagObject.transform;
			if (!Payload.FoundTags.Contains(tag)) Payload.FoundTags.Add(tag);

		}
  		#endregion

		#region Idle
		protected override void Idle()
		{
			App.SM.PushBlocking(InitializeCamera);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeMenu);
			//new CursorPresenter().Show();
		}

		void InitializeCamera(Action done)
		{
			new CameraPresenter().Show(done);
		}

		void InitializeInput(Action done)
		{
			App.Input.SetEnabled(true);
			done();
		}

		void InitializeMenu(Action done)
		{
			new HomeMenuPresenter().Show(done);
		}
		#endregion

		#region End
		protected override void End()
		{
			App.Input.SetEnabled(false);
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
			SceneManager.UnloadSceneAsync(SceneConstants.Home);
		}

		void OnSceneUnloaded(Scene scene)
		{
			App.Callbacks.SceneUnload -= OnSceneUnloaded;
			Payload.UnloadSceneCallback();
		}
  		#endregion
	}
}