using System;
using System.Linq;
using System.Collections.Generic;

using LunraGames.NumberDemon;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Presenters;

using UnityEngine;
using UnityEngine.Analytics;

namespace LunraGames.SubLight
{
	public class HomePayload : IStatePayload 
	{
		#region Required
		public HoloRoomFocusCameraPresenter MainCamera;
		#endregion

		/// <summary>
		/// Are we entering the home state for the first time?
		/// </summary>
		public bool FromInitialization;
		/// <summary>
		/// Did a game request us to automatically restart?
		/// </summary>
		public bool AutoRetryNewGame;
		public CreateGameBlock AutoRetryNewGameBlock;

		public bool CanContinueSave { get { return ContinueSave != null; } }
		public GameModel ContinueSave;
		public CreateGameBlock NewGameBlock;

		public float MenuAnimationMultiplier;
		public Dictionary<float, IPresenterCloseShowOptions[]> DelayedPresenterShows = new Dictionary<float, IPresenterCloseShowOptions[]>();

		public GalaxyPreviewModel DefaultGalaxy;
		public GamemodeInfoModel DefaultGamemode;
		public List<GamemodeInfoModel> Gamemodes = new List<GamemodeInfoModel>();

		public Action<GameModel, bool, bool> StartGame;

		public bool IgnoreMaskOnEnd;

		public ChangelogPresenter Changelog;
		public GamemodePortalPresenter GamemodePortal;

		public Action<bool> ToggleMainMenu;
	}

	public partial class HomeState : State<HomePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Home; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Home, SceneConstants.HoloRoom }; } }

		bool IsDeveloperAutoGameActive
		{
			get
			{
				switch (DevPrefs.AutoGameOption.Value)
				{
					case AutoGameOptions.NewGame:
					case AutoGameOptions.ContinueGame:
						return Payload.FromInitialization || DevPrefs.AutoGameOptionRepeats;
				}
				return false;
			}
		}

		#region Begin
		protected override void Begin()
		{
			Payload.MenuAnimationMultiplier = DevPrefs.SkipMainMenuAnimations ? 0f : 1f;
			Payload.StartGame = OnStartGame;

			// -- Required
			SM.PushBlocking(LoadScenes, "LoadScenes");
			SM.PushBlocking(InitializeInput, "InitializeInput");
			SM.PushBlocking(InitializeCallbacks, "InitializeCallbacks");
			SM.PushBlocking(InitializeLoadGalaxy, "InitializeLoadGalaxy");
			SM.PushBlocking(InitializeGamemodes, "InitializeGamemodes");
			SM.PushBlocking(InitializeNewGameBlock, "InitializingNewGameBlock");
			SM.PushBlocking(InitializeContinueGame, "InitializeContinueGame");
			SM.PushBlocking(done => Focuses.InitializePresenters(Payload, done), "InitializePresenters");
			SM.PushBlocking(InitializeFocus, "InitializeFocus");
		}
		#endregion

		#region Begin Required
		void LoadScenes(Action done)
		{
			App.Scenes.Request(SceneRequest.Load(result => done(), Scenes));
		}

		void InitializeInput(Action done)
		{
			App.Input.SetEnabled(true);
			done();
		}

		void InitializeCallbacks(Action done)
		{
			App.Callbacks.DialogRequest += OnDialogRequest;

			done();
		}

		void InitializeLoadGalaxy(Action done)
		{
			App.M.List<GalaxyPreviewModel>(result => OnListInitializeLoadGalaxy(result, done));
		}

		void OnListInitializeLoadGalaxy(SaveLoadArrayRequest<SaveModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable to load a list of galaxies");
				done();
				return;
			}

			if (string.IsNullOrEmpty(App.BuildPreferences.DefaultGalaxyId)) Debug.LogError("DefaultGalaxyId in BuildPreferences is null or empty, unpredictable behaviour may occur");
			var defaultGalaxy = result.Models.FirstOrDefault(m => m.GetMetaKey(MetaKeyConstants.GalaxyInfo.GalaxyId) == App.BuildPreferences.DefaultGalaxyId);
			
			if (defaultGalaxy == null)
			{
				Debug.LogError("No default galaxy with id \"" + App.BuildPreferences.DefaultGalaxyId + "\" was found");
				done();
				return;
			}

			App.M.Load<GalaxyPreviewModel>(defaultGalaxy, loadResult => OnLoadInitializeLoadGalaxy(loadResult, done));
		}

		void OnLoadInitializeLoadGalaxy(SaveLoadRequest<GalaxyPreviewModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable load galaxy at path: "+result.Model.Path+"\nError: "+result.Error);
				done();
				return;
			}

			Payload.DefaultGalaxy = result.TypedModel;

			done();
		}

		void InitializeGamemodes(Action done)
		{
			App.M.List<GamemodeInfoModel>(result => OnListInitializeGamemodes(result, done));
		}

		void OnListInitializeGamemodes(SaveLoadArrayRequest<SaveModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable to load a list of gamemodes");
				done();
				return;
			}

			OnLoadInitializeGamemodes(
				null,
				result.Models.ToList(),
				done
			);
		}

		void OnLoadInitializeGamemodes(
			SaveLoadRequest<GamemodeInfoModel>? result,
			List<SaveModel> remaining,
			Action done
		)
		{
			if (result.HasValue)
			{
				if (result.Value.Status != RequestStatus.Success) Debug.LogError("Unable to load gamemode with error " + result.Value.Error);
				else Payload.Gamemodes.Add(result.Value.TypedModel);
			}

			if (remaining.None())
			{
				if (string.IsNullOrEmpty(App.BuildPreferences.DefaultGamemodeId)) Debug.LogError("DefaultGamemodeId in BuildPreferences is null or empty, unpredictable behaviour may occur");
				Payload.DefaultGamemode = Payload.Gamemodes.FirstOrDefault(g => g.GetMetaKey(MetaKeyConstants.GamemodeInfo.GamemodeId) == App.BuildPreferences.DefaultGamemodeId);

				if (Payload.DefaultGamemode == null) Debug.LogError("No default gamemode with id \"" + App.BuildPreferences.DefaultGamemodeId + "\" was found");

				done();
				return;
			}

			var next = remaining.First();
			remaining.RemoveAt(0);

			App.M.Load<GamemodeInfoModel>(next, loadResult => OnLoadInitializeGamemodes(loadResult, remaining, done));
		}

		void InitializeNewGameBlock(Action done)
		{
			if (Payload.DefaultGamemode == null)
			{
				Debug.LogError("Unable to initialize the new game block with a default gamemode");
				done();
				return;
			}
			if (Payload.DefaultGalaxy == null)
			{
				Debug.LogError("Unable to initialize the new game block without a default galaxy");
				done();
				return;
			}

			var gameBlock = new CreateGameBlock
			{
				GameSeed = DemonUtility.NextInteger,

				GamemodeId = Payload.DefaultGamemode.GamemodeId.Value,

				GalaxySeed = DemonUtility.NextInteger,
				GalaxyId = Payload.DefaultGalaxy.GalaxyId.Value
				//GalaxyTargetId = TODO: Figure out how this should obtain this value...

				// Any other values should only be set if specified by developer preferences...
			};

			if (Payload.FromInitialization || DevPrefs.AutoGameOptionRepeats)
			{
				switch (DevPrefs.AutoGameOption.Value)
				{
					case AutoGameOptions.NewGame:
					case AutoGameOptions.OverrideGame:
						Debug.LogWarning("Developer Auto Game or Override Specified");
						DevPrefs.GameSeed.Set(ref gameBlock.GameSeed);
						DevPrefs.GamemodeId.Set(ref gameBlock.GamemodeId);
						DevPrefs.GalaxySeed.Set(ref gameBlock.GalaxySeed);
						DevPrefs.GalaxyId.Set(ref gameBlock.GalaxyId);
						DevPrefs.ToolbarSelection.Set(ref gameBlock.ToolbarSelection);
						break;
				}
			}
			else if (Payload.AutoRetryNewGame)
			{
				gameBlock.GamemodeId = Payload.AutoRetryNewGameBlock.GamemodeId;

				gameBlock.GalaxyId = Payload.AutoRetryNewGameBlock.GalaxyId;
				gameBlock.GalaxyTargetId = Payload.AutoRetryNewGameBlock.GalaxyTargetId;
			}

			Payload.NewGameBlock = gameBlock;

			done();
		}
		#endregion

		#region Begin Default
		void InitializeContinueGame(Action done)
		{
			App.GameService.ContinueGame((result, model) => OnInitializeContinueGame(result, model, done));
		}

		void OnInitializeContinueGame(RequestResult result, GameModel model, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable to load a continuable game");
				// TODO: Error logic.
			}
			else Payload.ContinueSave = model;

			done();
		}

		void InitializeFocus(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Default(Focuses.GetDefaultFocuses(), () => OnInializeFocusDefaults(done)));
		}

		void OnInializeFocusDefaults(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(Focuses.GetMainMenuFocus(), done));
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			if (IsDeveloperAutoGameActive)
			{
				switch (DevPrefs.AutoGameOption.Value)
				{
					case AutoGameOptions.NewGame:
						App.GameService.CreateGame(Payload.NewGameBlock, OnCreateGame);
						return;
					case AutoGameOptions.ContinueGame:
						App.GameService.ContinueGame(OnContinueGame);
						return;
				}
			}

			if (Payload.AutoRetryNewGame)
			{
				App.GameService.CreateGame(Payload.NewGameBlock, OnCreateGame);
				return;
			}

			PushIdleDefaults();
		}

		#endregion

		#region Idle Auto New Game or Continue
		void OnCreateGame(RequestResult result, GameModel model)
		{
			if (result.IsNotSuccess)
			{
				Debug.LogError("Falling back to main menu, creating new game returned result " + result.Status + " error: " + result.Message);
				PushIdleDefaults();
				return;
			}
			Payload.StartGame(model, true, false);
		}

		void OnContinueGame(RequestResult result, GameModel model)
		{
			if (result.IsNotSuccess)
			{
				Debug.LogError("Falling back to main menu, continuing game returned result " + result.Status + " error: " + result.Message);
				PushIdleDefaults();
				return;
			}

			if (model == null)
			{
				Debug.LogError("Falling back to main menu, no game to continue");
				PushIdleDefaults();
				return;
			}

			Payload.StartGame(model, true, true);
		}
		#endregion

		#region Idle Defaults
		void PushIdleDefaults()
		{
			SM.Push(
				() =>
				{
					App.Callbacks.HoloColorRequest(new HoloColorRequest(new Color(1f, 0.25f, 0.11f)));
					var finalRevealDuration = Payload.MenuAnimationMultiplier * CameraMaskRequest.DefaultRevealDuration;
					App.Audio.SetSnapshot(AudioSnapshots.Master.Home.MainMenu, finalRevealDuration * 2f);
					App.Callbacks.CameraMaskRequest(CameraMaskRequest.Reveal(finalRevealDuration, OnIdleShow));
				},
				"HomeIdleDefaults"
			);
		}

		void OnIdleShow()
		{
			if (DevPrefs.HideMainMenu.Value) return;

			var totalWait = 0f;
			foreach (var kv in Payload.DelayedPresenterShows)
			{
				var currWait = kv.Key * Payload.MenuAnimationMultiplier;
				totalWait += currWait;
				App.Heartbeat.Wait(
					() =>
					{
						foreach (var presenter in kv.Value) presenter.Show(instant: Mathf.Approximately(0f, Payload.MenuAnimationMultiplier));
					},
					currWait
				);
			}

			App.Heartbeat.Wait(
				OnCheckForAnalyticsWarning,
				totalWait + 0.001f
			);
		}

		void OnCheckForAnalyticsWarning()
		{
			if (App.MetaKeyValues.Get(KeyDefines.Global.IgnoreUserAnalyticsWarning))
			{
				OnIdleShowComplete();
				return;
			}

			App.Callbacks.DialogRequest(
				DialogRequest.ConfirmDeny(
					LanguageStringModel.Override("<b>SubLight</b> uses analytics to fix bugs and improve your experience playing our game! " +
					                             "We do not share or sell personal data about you to anyone, but we understand if you would " +
					                             "still like to opt out of analytics."),
					DialogStyles.Neutral,
					LanguageStringModel.Override("Analytics"),
					() => OnSetAnalytics(true),
					() => OnSetAnalytics(false),
					LanguageStringModel.Override("Keep Enabled"),
					LanguageStringModel.Override("Disable")
				)
			);
		}

		void OnSetAnalytics(bool analyticsEnabled)
		{
			Analytics.enabled = analyticsEnabled;
			App.MetaKeyValues.Set(KeyDefines.Global.IgnoreUserAnalyticsWarning, true);

			App.MetaKeyValues.Save(
				result =>
				{
					if (result.IsNotSuccess) Debug.LogError("Unable to save meta keyvalues, status " + result.Status + " with error: " + result.Message);
					OnIdleShowComplete();
				}
			);
		}

		void OnIdleShowComplete()
		{
			if (Payload.FromInitialization)
			{
				var previousVersion = App.MetaKeyValues.Get(KeyDefines.Global.PreviousChangelogVersion);
				var currentVersion = App.BuildPreferences.Current.Version;
				App.MetaKeyValues.Set(KeyDefines.Global.PreviousChangelogVersion, currentVersion);

				App.MetaKeyValues.Save(
					result =>
					{
						if (result.Status == RequestStatus.Success && previousVersion != 0 && previousVersion < currentVersion)
						{
							ShowChangelog();
						}
					}
				);


			}
			else
			{
				if (!App.MetaKeyValues.Get(KeyDefines.Global.IgnoreFeedbackRequest)) AskForFeedback();
			}
		}

		void ShowChangelog()
		{
			var changeLog = App.BuildPreferences.Current;
			App.MetaKeyValues.Set(KeyDefines.Global.PreviousChangelogVersion, changeLog.Version);

			Payload.Changelog.Show(
				setFocusInstant =>
				{
					if (setFocusInstant) App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(Focuses.GetPriorityFocus()));
					else App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetPriorityFocus()));
				},
				() =>
				{
					App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetMainMenuFocus()));
				}
			);
		}

		void AskForFeedback()
		{
			App.MetaKeyValues.Set(KeyDefines.Global.IgnoreFeedbackRequest, true);
			App.Callbacks.DialogRequest(
				DialogRequest.ConfirmDeny(
					LanguageStringModel.Override("If you're interested in providing feedback, it would help us out a lot!"),
					title: LanguageStringModel.Override("Submit Feedback?"),
					confirmClick: OnSubmitFeedbackClick
				)
			);
		}

		void OnSubmitFeedbackClick()
		{
			App.Heartbeat.Wait(
				() =>
				{
					Application.OpenURL(
						App.BuildPreferences.FeedbackForm(
							FeedbackFormTriggers.MainMenu,
							App.MetaKeyValues.GlobalKeyValues
						)
					);
				},
				0.75f
			);

			App.Callbacks.DialogRequest(
				DialogRequest.Confirm(
					LanguageStringModel.Override("Your browser should open to a feedback form, if not visit <b>strangestar.games/contact</b> to send us a message!"),
					DialogStyles.Neutral,
					LanguageStringModel.Override("Feedback")
				)
			);
		}
		#endregion

		#region End
		protected override void End()
		{
			App.Callbacks.DialogRequest -= OnDialogRequest;

			App.Input.SetEnabled(false);

			if (!Payload.IgnoreMaskOnEnd)
			{
				SM.PushBlocking(
					done => App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetNoFocus(), done)),
					"HomeSettingNoFocus"
				);

				SM.PushBlocking(
					done => App.Callbacks.CameraMaskRequest(CameraMaskRequest.Hide(CameraMaskRequest.DefaultHideDuration, done)),
					"HomeHideMask"
				);
			}

			SM.PushBlocking(
				done => App.P.UnRegisterAll(done),
				"HomeUnBind"
			);

			SM.PushBlocking(
				done => App.Scenes.Request(SceneRequest.UnLoad(result => done(), Scenes)),
				"HomeUnLoadScenes"
			);
		}
		#endregion

		#region Events
		void OnDialogRequest(DialogRequest request)
		{
			if (request.OverrideFocusHandling) return;
			switch (request.State)
			{
				case DialogRequest.States.Request:
					App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetPriorityFocus()));
					App.Audio.SetSnapshot(AudioSnapshots.Master.Shared.Paused);
					break;
				case DialogRequest.States.Completing:
					App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetMainMenuFocus()));
					App.Audio.SetSnapshot(AudioSnapshots.Master.Home.MainMenu);
					break;
			}
		}

		void OnStartGame(
			GameModel model,
			bool instant,
			bool isContinue
		)
		{
			if (App.MetaKeyValues.Get(KeyDefines.Preferences.IsDemoMode))
			{
				Debug.LogWarning("Starting or continuing game in Demo Mode.");
				App.MetaKeyValues.Set(KeyDefines.Preferences.IgnoreTutorial, false);
			}

			Payload.IgnoreMaskOnEnd = instant;

			if (isContinue) App.Analytics.GameContinue(model);
			else App.Analytics.GameStart(model);

			SM.Push(
				() =>
				{
					Debug.Log("Starting game...");
					var gamePayload = new GamePayload
					{
						MainCamera = Payload.MainCamera,
						Game = model
					};
					App.SM.RequestState(gamePayload);
				},
				"StartGameRequestState"
			);

			App.Audio.SetSnapshot(AudioSnapshots.Master.Shared.Transition);
		}
		#endregion
	}
}