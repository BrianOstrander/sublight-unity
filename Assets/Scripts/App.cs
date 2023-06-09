﻿using System;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class App
	{
		static App instance;

		public static bool HasInstance { get { return instance != null; } }

		/// <summary>
		/// Called when the App instance is done being constructed.
		/// </summary>
		/// <remarks>
		/// Idealy nothing should use this except editor time scripts.
		/// </remarks>
		public static Action<App> Instantiated = ActionExtensions.GetEmpty<App>();

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

		EncounterHandlerService encounterHandler;
		public static EncounterHandlerService EncounterHandler { get { return instance.encounterHandler; } }

		KeyValueService keyValues;
		public static KeyValueService KeyValues { get { return instance.keyValues; } }

		MetaKeyValueService metaKeyValues;
		public static MetaKeyValueService MetaKeyValues { get { return instance.metaKeyValues; } }

		ValueFilterService valueFilter;
		public static ValueFilterService ValueFilter { get { return instance.valueFilter; } }

		FocusService focus;
		public static FocusService Focus { get { return instance.focus; } }

		AnalyticsService analytics;
		public static AnalyticsService Analytics { get { return instance.analytics; } }

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
		/// preserved when saving. Don't provide this model to any services
		/// before initialization though, since it will be replaced.
		/// </remarks>
		/// <value>The preferences.</value>
		public static PreferencesModel Preferences { get { return instance.preferences; } }

		static Func<PreferencesModel> CurrentPreferences { get { return () => Preferences; } }

		public App(
			Main main, 
			DefaultShaderGlobals shaderGlobals, 
			List<GameObject> defaultViews, 
			BuildPreferences buildPreferences,
			GameObject audioRoot, 
			Transform canvasRoot,
			Transform gameCanvasRoot,
			Transform overlayCanvasRoot,
			SceneSkybox sceneSkybox,
			AudioConfiguration audioConfiguration
		)
		{
			Time.timeScale = DevPrefs.ApplyTimeScaling.Value ? DevPrefs.TimeScaling.Value : 1f;

			if (DevPrefs.ApplyTimeScaling.Value) Debug.LogWarning("DevPref: Time Scaling to " + DevPrefs.TimeScaling.Value.ToString("N2"));

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
			viewMediator = new ViewMediator(Heartbeat, Callbacks);
			stateMachine = new StateMachine(
				Heartbeat,
				new InitializeState(),
				new TransitionState(),
				new HomeState(),
				new GameState()
			);
			universe = new FudgedUniverseService();

			if (Application.isEditor)
			{
#if UNITY_EDITOR
				input = new EditorInputService(Heartbeat, Callbacks);
				modelMediator = new DesktopModelMediator();
#endif
			}
			else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				input = new DesktopInputService(Heartbeat, Callbacks);
				modelMediator = new DesktopModelMediator();
			}
			else
			{
				throw new Exception("Unknown platform");
			}

			scenes = new SceneService(Callbacks, sceneSkybox);
			gameService = new GameService(M, Universe);
			keyValues = new KeyValueService(Callbacks);
			metaKeyValues = new MetaKeyValueService(Callbacks, M, KeyValues);
			valueFilter = new ValueFilterService(Callbacks);
			encounters = new EncounterService(M, Callbacks, ValueFilter);

			encounterHandler = new EncounterHandlerService(
				Heartbeat,
				Callbacks,
				Encounters,
				KeyValues,
				ValueFilter,
				CurrentPreferences,
				SM,
				new DialogLanguageBlock
				{
					Title = LanguageStringModel.Override("Active Encounter"),
					Message = LanguageStringModel.Override("Saving is disabled during encounters.")
				}
			);

			focus = new FocusService(Heartbeat, Callbacks);
			analytics = new AnalyticsService(
				Callbacks,
				BuildPreferences
			);

			audioService = new AudioService(
				audioRoot, 
				audioConfiguration, 
				Heartbeat,
				MetaKeyValues,
				BuildPreferences
			);

			Application.targetFrameRate = BuildPreferences.TargetFrameRate;
			QualitySettings.vSyncCount = BuildPreferences.VSyncCount;

			Instantiated(this);
		}

		public static void Restart(string message)
		{
			Debug.LogError("NO RESTART LOGIC DEFINED - TRIGGERED BY:\n" + message);
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
	}
}
