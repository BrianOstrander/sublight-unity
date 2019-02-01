using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct SceneRequest
	{
		public enum Types
		{
			Unknown = 0,
			Load = 10,
			UnLoad = 20
		}

		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		public static SceneRequest Load(Action<SceneRequest> done, string[] scenes, string[] tags = null)
		{
			return new SceneRequest(done, scenes, tags, Types.Load, States.Request);
		}

		public static SceneRequest UnLoad(Action<SceneRequest> done, string[] scenes)
		{
			return new SceneRequest(done, scenes, new string[0], Types.UnLoad, States.Request);
		}

		SceneRequest(Action<SceneRequest> done, string[] scenes, string[] tags, Types type, States state)
		{
			Done = done;
			Scenes = scenes;
			Tags = tags ?? new string[0];

			Type = type;
			State = state;
			ProcessedScenes = new List<string>();
			MissingTags = new List<string>();
			FoundTags = new Dictionary<string, GameObject>();
		}

		public Action<SceneRequest> Done;
		public string[] Scenes;
		public string[] Tags;

		public Types Type;
		public States State;
		public List<string> ProcessedScenes;
		public List<string> MissingTags;
		public Dictionary<string, GameObject> FoundTags;
	}

	public class SceneService
	{
		ILogService logging;
		CallbackService callbacks;
		SceneSkybox sceneSkybox;

		SceneRequest current;

		GameModel model;

		public SceneService(
			ILogService logging,
			CallbackService callbacks,
			SceneSkybox sceneSkybox
		)
		{
			if (logging == null) throw new ArgumentNullException("logging");
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (sceneSkybox == null) throw new ArgumentNullException("sceneSkybox");

			this.logging = logging;
			this.callbacks = callbacks;
			this.sceneSkybox = sceneSkybox;

			callbacks.StateChange += OnStateChange;
		}

		#region Exposed Functionality
		public void Request(SceneRequest request)
		{
			if (request.State != SceneRequest.States.Request) throw new ArgumentOutOfRangeException("request.State", "Must be a States.Request");
			switch (current.State)
			{
				case SceneRequest.States.Request:
				case SceneRequest.States.Active:
					throw new Exception("Cannot start a request before the last one is complete");
			}
			current = request;
			switch (current.Type)
			{
				case SceneRequest.Types.Load:
					LoadScenes();
					break;
				case SceneRequest.Types.UnLoad:
					UnloadScene();
					break;
			}
		}
		#endregion

		#region Load Scenes
		void LoadScenes()
		{
			logging.Log("Loading Scenes", LogTypes.Initialization);
			callbacks.SceneLoad += OnSceneLoaded;
			foreach (var scene in current.Scenes) SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		}

		void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
		{
			if (!current.Scenes.Contains(scene.name)) return;

			logging.Log("Loaded Scene: " + scene.name, LogTypes.Initialization);

			current.ProcessedScenes.Add(scene.name);

			if (current.Scenes.Length != current.ProcessedScenes.Count) return;

			logging.Log("All Scenes Loaded", LogTypes.Initialization);

			foreach (var tag in current.Tags)
			{
				if (current.FoundTags.ContainsKey(tag) || current.MissingTags.Contains(tag)) continue;
				var tagObject = GameObjectExtensions.FindWithTagOrHandleMissingTag(tag);
				if (tagObject == null)
				{
					current.MissingTags.Add(tag);
					continue;
				}
				current.FoundTags.Add(tag, tagObject);
			}

			if (0 < current.MissingTags.Count())
			{
				var tagWarning = "Missing Tags:";
				foreach (var tag in current.MissingTags) tagWarning += "\n\t" + tag;
				Debug.LogError(tagWarning);
			}

			callbacks.SceneLoad -= OnSceneLoaded;
			callbacks.SceneSetActive += OnAllScenesLoaded;

			SceneManager.SetActiveScene(scene);
		}

		void OnAllScenesLoaded(Scene currentScene, Scene nextScene)
		{
			callbacks.SceneSetActive -= OnAllScenesLoaded;
			sceneSkybox.ApplyRenderSettings();
			OnAllScenesProcessed();
		}
		#endregion

		#region UnLoad Scenes
		void UnloadScene()
		{
			callbacks.SceneUnload += OnSceneUnloaded;
			foreach (var scene in current.Scenes) SceneManager.UnloadSceneAsync(scene);
		}

		void OnSceneUnloaded(Scene scene)
		{
			if (!current.Scenes.Contains(scene.name)) return;

			logging.Log("Unloaded Scene: " + scene.name, LogTypes.Initialization);

			current.ProcessedScenes.Add(scene.name);

			if (current.Scenes.Length != current.ProcessedScenes.Count) return;

			logging.Log("All Scenes Unloaded", LogTypes.Initialization);

			callbacks.SceneUnload -= OnSceneUnloaded;
			OnAllScenesProcessed();
		}
		#endregion
		void OnAllScenesProcessed()
		{
			//sceneSkybox.ApplyRenderSettings();
			current.State = SceneRequest.States.Complete;
			if (current.Done != null) current.Done(current);
		}

		#region Events
		void OnStateChange(StateChange change)
		{
			switch (change.State)
			{
				case StateMachine.States.Game:
					switch (change.Event)
					{
						case StateMachine.Events.Begin:
							OnGameBegin(change);
							break;
						case StateMachine.Events.End:
							OnGameEnd(change);
							break;
					}
					break;
			}
		}

		void OnGameBegin(StateChange change)
		{
			model = (change.Payload as GamePayload).Game;
			model.TransitState.Changed += OnTransitState;
			OnTransitState(model.TransitState);
		}

		void OnGameEnd(StateChange change)
		{
			model.TransitState.Changed -= OnTransitState;
			OnTransitState(TransitState.Default());
			model = null;
		}

		void OnTransitState(TransitState transitState)
		{
			sceneSkybox.TimeScalar = transitState.RelativeTimeScalar;
		}
		#endregion
	}
}