﻿using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

using LunraGames.SpaceFarm.Presenters;
using LunraGames.SpaceFarm;

namespace LunraGames.SpaceFarm 
{
	public class App
	{
		static App instance;

		Main main;
		private bool cvMode;
		public static bool CVMode
		{
			get { return instance.cvMode; }
		}
		public static Main Main { get { return instance.main; } }

		CallbackService callbackService;
		public static CallbackService Callbacks { get { return instance.callbackService; } }

		Heartbeat heartbeat;
		public static Heartbeat Heartbeat { get { return instance.heartbeat; } }

		ModelMediator modelMediator;
		public static ModelMediator M { get { return instance.modelMediator; } }

		PresenterMediator presenterMediator;
		public static PresenterMediator P { get { return instance.presenterMediator; } }

		ViewMediator viewMediator;
		public static ViewMediator V { get { return instance.viewMediator; } }

		AudioService audioService;
		public static AudioService Audio { get { return instance.audioService; } }

		StateMachine stateMachine;
		public static StateMachine SM { get { return instance.stateMachine; } }

		ILogService logService;
		public static ILogService Logging { get { return instance.logService; } }

		IInputService inputService;
		public static IInputService Input { get { return instance.inputService; } }

		IBackendService backendService;
		public static IBackendService BackendService { get { return instance.backendService; } }

		List<GameObject> defaultViews;
		DefaultShaderGlobals shaderGlobals;

		public App(Main main, DefaultShaderGlobals shaderGlobals, List<GameObject> defaultViews, GameObject audioRoot, bool _cvMode)
		{
			instance = this;
			cvMode = _cvMode;
			this.main = main;
			this.defaultViews = defaultViews;
			this.shaderGlobals = shaderGlobals;
			callbackService = new CallbackService();
			heartbeat = new Heartbeat();
			modelMediator = new ModelMediator();
			presenterMediator = new PresenterMediator();
			viewMediator = new ViewMediator();
			audioService = new AudioService(audioRoot);
			stateMachine = new StateMachine(
				new InitializeState(),
				new HomeState()
			);

			if (Application.isEditor)
			{
				logService = new EditorLogService();
				inputService = new EditorInputService();
				backendService = new EditorBackendService();
			}
			else if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				// Desktop
				logService = new WebGlLogService();
				inputService = new WebGlInputService();
				backendService = new WebGlBackendService();
			}
			else
			{
				throw new Exception("Unknown platform");
			}
		}

		public static void Restart(string message)
		{
			Debug.LogError(message);
			throw new NotImplementedException();
		}

		#region MonoBehaviour events

		public void Awake()
		{
			var payload = new InitializePayload();
			payload.DefaultViews = defaultViews;
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
			if (instance != null && instance.logService != null) Logging.Log(message, logType, context, onlyOnce);
		}

		#endregion
	}
}