using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace LunraGames.SpaceFarm
{
	public class InitializePayload : IStatePayload
	{
		public List<GameObject> DefaultViews = new List<GameObject>();
	}

	public class InitializeState : State<InitializePayload>
	{
		public override StateMachine.States HandledState { get { return StateMachine.States.Initialize; } }

		static string[] Scenes {
			get 
			{
				var scenes = new List<string> (new string[] {
					//"SomeScene", "AnotherScene"
				});
				return scenes.ToArray();
			}
		}

		List<string> loadedScenes = new List<string>();
		List<string> checkedTags = new List<string>();
		List<string> foundTags = new List<string>();
		Action initializeSceneCallback = ActionExtensions.Empty;
		HomePayload homePayload = new HomePayload();

		protected override void Begin()
		{
			App.SM.PushBlocking(InitializeViews);
			App.SM.PushBlocking(InitializeModels);
			App.SM.PushBlocking(InitializePresenters);
			App.SM.PushBlocking(InitializeScenes);
			App.SM.PushBlocking(InitializeInput);
		}

		protected override void Idle()
		{
			App.SM.RequestState(homePayload);
		}

		#region Mediators
		void InitializeViews(Action callback)
		{
			App.V.Initialize(
				Payload.DefaultViews,
				App.Main.transform,
				status =>
				{
					if (status == RequestStatus.Success) App.Log("ViewMediator Initialized", LogTypes.Initialization);
					else App.Restart("Initializing ViewMediator failed with status " + status);

					callback();
				}
			);
		}

		void InitializeModels(Action callback)
		{
			App.M.Initialize(
				status =>
				{
					if (status == RequestStatus.Success) App.Log("ModelMediator Initialized", LogTypes.Initialization);
					else App.Restart("Initializing ModelMediator failed with status " + status);

					callback();
				}
			);
		}

		void InitializePresenters(Action callback)
		{
			App.P.Initialize(
				status =>
				{
					if (status == RequestStatus.Success) App.Log("PresenterMediator Initialized", LogTypes.Initialization);
					else App.Restart("Initializing PresenterMediator failed with status " + status);

					callback();
				}
			);
		}
		#endregion

		void InitializeScenes(Action callback)
		{
			App.Log("Loading Scenes", LogTypes.Initialization);
			initializeSceneCallback = callback;
			App.Callbacks.SceneLoad += OnSceneInitialized;
			foreach (var scene in Scenes) SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		}

		void OnSceneInitialized(Scene scene, LoadSceneMode loadMode)
		{
			if (!Scenes.Contains(scene.name)) return;

			App.Log("Loaded Scene: "+scene.name, LogTypes.Initialization);

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

		void InitializeInput(Action callback)
		{
			App.Input.SetEnabled(true);
			callback();
		}

		void AssignTag(ref Transform existing, string tag)
		{
			if (existing != null) return;

			if (!checkedTags.Contains(tag)) checkedTags.Add(tag);
			var tagObject = GameObject.FindWithTag(tag);

			if (tagObject == null) return;

			existing = tagObject.transform;
			if (!foundTags.Contains(tag)) foundTags.Add(tag);

		}
	}
}