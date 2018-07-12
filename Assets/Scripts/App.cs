﻿using System;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class App
	{
		static App instance;

		Main main;
		public static Main Main { get { return instance.main; } }

		CallbackService callbackService;
		public static CallbackService Callbacks { get { return instance.callbackService; } }

		Heartbeat heartbeat;
		public static Heartbeat Heartbeat { get { return instance.heartbeat; } }

		IModelMediator modelMediator;
		public static IModelMediator M { get { return instance.modelMediator; } }

		PresenterMediator presenterMediator;
		public static PresenterMediator P { get { return instance.presenterMediator; } }

		ViewMediator viewMediator;
		public static ViewMediator V { get { return instance.viewMediator; } }

		AudioService audioService;
		public static AudioService Audio { get { return instance.audioService; } }

		StateMachine stateMachine;
		public static StateMachine SM { get { return instance.stateMachine; } }

		ILogService logging;
		public static ILogService Logging { get { return instance.logging; } }

		IInputService input;
		public static IInputService Input { get { return instance.input; } }

		SceneService scenes;
		public static SceneService Scenes { get { return instance.scenes; } }

		IUniverseService universe;
		public static IUniverseService Universe { get { return instance.universe; } }

		GameService gameService;
		public static GameService GameService { get { return instance.gameService; } }

		EncounterService encounters;
		public static EncounterService Encounters { get { return instance.encounters; } }

		List<GameObject> defaultViews;
		DefaultShaderGlobals shaderGlobals;

		BuildPreferences buildPreferences;
		public static BuildPreferences BuildPreferences { get { return instance.buildPreferences; } }

		// TODO: Should this be here?
		Transform canvasRoot;
		public static Transform CanvasRoot { get { return instance.canvasRoot; } }
		Transform gameCanvasRoot;
		public static Transform GameCanvasRoot { get { return instance.gameCanvasRoot; } }
		Transform overlayCanvasRoot;
		public static Transform OverlayCanvasRoot { get { return instance.overlayCanvasRoot; } }

		PreferencesModel preferences;
		/// <summary>
		/// Gets the current preferences.
		/// </summary>
		/// <remarks>
		/// Feel free to hook onto the Changed listeners for this model, they're
		/// preserved when saving.
		/// </remarks>
		/// <value>The preferences.</value>
		public static PreferencesModel Preferences { get { return instance.preferences; } }

		public App(
			Main main, 
			DefaultShaderGlobals shaderGlobals, 
			List<GameObject> defaultViews, 
			BuildPreferences buildPreferences,
			GameObject audioRoot, 
			Transform canvasRoot,
			Transform gameCanvasRoot,
			Transform overlayCanvasRoot
		)
		{
			instance = this;
			this.main = main;
			this.defaultViews = defaultViews;
			this.shaderGlobals = shaderGlobals;
			this.buildPreferences = buildPreferences;
			this.canvasRoot = canvasRoot;
			this.gameCanvasRoot = gameCanvasRoot;
			this.overlayCanvasRoot = overlayCanvasRoot;
			callbackService = new CallbackService();
			heartbeat = new Heartbeat();
			presenterMediator = new PresenterMediator(Heartbeat);
			viewMediator = new ViewMediator(Heartbeat);
			audioService = new AudioService(audioRoot);
			stateMachine = new StateMachine(
				Heartbeat,
				new InitializeState(),
				new HomeState(),
				new GameState()
			);
			universe = new FudgedUniverseService();

			if (Application.isEditor)
			{
#if UNITY_EDITOR
				logging = new EditorLogService();
				input = new EditorInputService(Heartbeat, Callbacks);
				modelMediator = new EditorSaveLoadService();
#endif
			}
			else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				logging = new DesktopLogService();
				input = new DesktopInputService(Heartbeat, Callbacks);
				modelMediator = new DesktopSaveLoadService();
			}
			else
			{
				throw new Exception("Unknown platform");
			}

			encounters = new EncounterService(modelMediator, logging);
			scenes = new SceneService(Logging, Callbacks);
			gameService = new GameService(M, Universe);
		}

		public static void Restart(string message)
		{
			Debug.LogError(message);
			throw new NotImplementedException();
		}

		#region Global setters
		/// <summary>
		/// Used to set the initial preferences. Should only be set once from
		/// the initialize state.
		/// </summary>
		/// <param name="preferences">Preferences.</param>
		public static void SetPreferences(PreferencesModel preferences) { instance.preferences = preferences; }
		#endregion

		#region MonoBehaviour events

		public void Awake()
		{
			var payload = new InitializePayload();
			payload.DefaultViews = defaultViews;
			payload.ShaderGlobals = shaderGlobals;
			stateMachine.RequestState(payload);
		}

		public void Start()
		{

		}

		public void Update(float delta)
		{
			heartbeat.Update(delta);
		}

		public void LateUpdate(float delta)
		{
			heartbeat.LateUpdate(delta);
		}

		public void FixedUpdate() { }

		public void OnApplicationPause(bool paused) { }

		public void OnApplicationQuit() { }

		#endregion

		#region Utility
		/// <summary>
		/// Log the specified info message, a convenience method for the LogService.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="logType">Log type.</param>
		/// <param name="context">Context.</param>
		/// <param name="onlyOnce">If set to <c>true</c> only once.</param>
		public static void Log(object message, LogTypes logType = LogTypes.Uncatagorized, Object context = null, bool onlyOnce = false)
		{
			if (instance != null && instance.logging != null) Logging.Log(message, logType, context, onlyOnce);
		}

		#endregion
	}
}
