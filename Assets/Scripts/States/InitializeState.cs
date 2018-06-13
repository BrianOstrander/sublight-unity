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
		public DefaultShaderGlobals ShaderGlobals;

		public List<string> loadedScenes = new List<string>();
		public List<string> checkedTags = new List<string>();
		public List<string> foundTags = new List<string>();
		public Action initializeSceneCallback = ActionExtensions.Empty;
		public HomePayload homePayload = new HomePayload();
	}

	public class InitializeState : State<InitializePayload>
	{
		/// Reminder: Keep local variables in the payload so it's easy to reset 
		/// states upon transition.

		public override StateMachine.States HandledState { get { return StateMachine.States.Initialize; } }

		static string[] Scenes {
			get 
			{
				var scenes = new List<string> (new string[] {
					"Home"
				});
				return scenes.ToArray();
			}
		}

		protected override void Begin()
		{
			Payload.ShaderGlobals.Apply();
			App.SM.PushBlocking(InitializeViews);
			App.SM.PushBlocking(InitializeModels);
			App.SM.PushBlocking(InitializePresenters);
			App.SM.PushBlocking(InitializeScenes);
		}

		protected override void Idle()
		{
			App.SM.RequestState(Payload.homePayload);
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
			Payload.initializeSceneCallback = callback;
			App.Callbacks.SceneLoad += OnSceneInitialized;
			foreach (var scene in Scenes) SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		}

		void OnSceneInitialized(Scene scene, LoadSceneMode loadMode)
		{
			if (!Scenes.Contains(scene.name)) return;

			App.Log("Loaded Scene: "+scene.name, LogTypes.Initialization);

			Payload.loadedScenes.Add(scene.name);
			SceneManager.SetActiveScene(scene);

			if (Scenes.Length != Payload.loadedScenes.Count) return;

			App.Log("All Scenes Loaded", LogTypes.Initialization);

			var missingTags = Payload.checkedTags.Where(t => !Payload.foundTags.Contains(t));
			if (0 < missingTags.Count())
			{
				var tagWarning = "Missing Tags:";
				foreach (var tag in missingTags) tagWarning += "\n\t" + tag;
				Debug.LogError(tagWarning);
			}

			App.Callbacks.SceneLoad -= OnSceneInitialized;
			Payload.initializeSceneCallback();
		}

		void AssignTag(ref Transform existing, string tag)
		{
			if (existing != null) return;

			if (!Payload.checkedTags.Contains(tag)) Payload.checkedTags.Add(tag);
			var tagObject = GameObject.FindWithTag(tag);

			if (tagObject == null) return;

			existing = tagObject.transform;
			if (!Payload.foundTags.Contains(tag)) Payload.foundTags.Add(tag);

		}
	}
}