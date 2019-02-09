using System;
using System.Linq;
using System.Collections.Generic;

using LunraGames.NumberDemon;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Presenters;

using UnityEngine;

namespace LunraGames.SubLight
{
	public class HomePayload : IStatePayload 
	{
		#region Required
		public HoloRoomFocusCameraPresenter MainCamera;
		#endregion

		public bool CanContinueSave { get { return ContinueSave != null; } }
		public SaveModel[] Saves = new SaveModel[0];
		public SaveModel ContinueSave;
		public CreateGameBlock NewGameBlock;

		public float MenuAnimationMultiplier;
		public Dictionary<float, IPresenterCloseShowOptions[]> DelayedPresenterShows = new Dictionary<float, IPresenterCloseShowOptions[]>();

		public GalaxyPreviewModel PreviewGalaxy;
	}

	public partial class HomeState : State<HomePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Home; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Home, SceneConstants.HoloRoom }; } }

		#region Begin
		protected override void Begin()
		{
			Payload.MenuAnimationMultiplier = DevPrefs.SkipMainMenuAnimations ? 0f : 1f;

			SM.PushBlocking(LoadScenes, "LoadScenes");
			SM.PushBlocking(InitializeInput, "InitializeInput");
			SM.PushBlocking(InitializeCallbacks, "InitializeCallbacks");
			SM.PushBlocking(InitializeLoadSaves, "InitializeLoadSaves");
			SM.PushBlocking(InitializeLoadGalaxy, "InitializeLoadGalaxy");
			SM.PushBlocking(done => Focuses.InitializePresenters(this, done), "InitializePresenters");
			SM.PushBlocking(InitializeFocus, "InitializeFocus");
		}

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

		void InitializeFocus(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Default(Focuses.GetDefaultFocuses(), () => OnInializeFocusDefaults(done)));
		}

		void OnInializeFocusDefaults(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.RequestInstant(Focuses.GetMainMenuFocus(), done));
		}

		void InitializeLoadSaves(Action done)
		{
			App.M.List<GameModel>(result => OnInitializeLoadSaves(result, done));
		}

		void OnInitializeLoadSaves(SaveLoadArrayRequest<SaveModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable to load a list of saved games");
				// TODO: Error logic.
			}
			else Payload.Saves = result.Models;

			Payload.ContinueSave = Payload.Saves.Where(s => s.SupportedVersion.Value).OrderBy(s => s.Modified.Value).LastOrDefault();

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

			var milkyWay = result.Models.FirstOrDefault(m => m.Meta == "Milky Way");
			
			if (milkyWay == null)
			{
				Debug.LogError("No galaxies named \"Milky Way\" were listed");
				done();
				return;
			}

			App.M.Load<GalaxyPreviewModel>(milkyWay, loadResult => OnLoadInitializeLoadGalaxy(loadResult, done));
		}

		void OnLoadInitializeLoadGalaxy(SaveLoadRequest<GalaxyPreviewModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable load galaxy at path: "+result.Model.Path+"\nError: "+result.Error);
				done();
				return;
			}

			Payload.PreviewGalaxy = result.TypedModel;

			done();
		}

		void InitializeNewGameBlock(Action done)
		{
			if (Payload.PreviewGalaxy == null)
			{
				Debug.LogError("Unable to initialize the new game block without a preview galaxy");
				done();
				return;
			}

			Payload.NewGameBlock = new CreateGameBlock
			{
				GameSeed = DemonUtility.NextInteger,
				GalaxySeed = DemonUtility.NextInteger,
				GalaxyId = Payload.PreviewGalaxy.GalaxyId.Value
			};

			done();
		}
		#endregion

		#region Idle
		protected override void Idle()
		{
			App.Callbacks.HoloColorRequest(new HoloColorRequest(new Color(1f, 0.25f, 0.11f)));
			App.Callbacks.CameraMaskRequest(CameraMaskRequest.Reveal(Payload.MenuAnimationMultiplier * CameraMaskRequest.DefaultRevealDuration, OnIdleShow));
		}

		void OnIdleShow()
		{
			foreach (var kv in Payload.DelayedPresenterShows)
			{
				App.Heartbeat.Wait(
					() =>
				{
					foreach (var presenter in kv.Value) presenter.Show(instant: Mathf.Approximately(0f, Payload.MenuAnimationMultiplier));
				},
					kv.Key * Payload.MenuAnimationMultiplier
				);
			}
		}
		#endregion

		#region End
		protected override void End()
		{
			App.Callbacks.DialogRequest -= OnDialogRequest;

			App.Input.SetEnabled(false);

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
					break;
				case DialogRequest.States.Completing:
					App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetMainMenuFocus()));
					break;
			}
		}
		#endregion

		#region Click Events
		void OnNewGameClick()
		{
			if (Payload.CanContinueSave) App.Callbacks.DialogRequest(DialogRequest.ConfirmDeny(LanguageStringModel.Override("Starting a new game will overwrite your existing one."), DialogStyles.Warning, LanguageStringModel.Override("Overwrite Game"), StartNewGame));
			else StartNewGame();
		}

		void OnContinueGameClick()
		{
			App.GameService.LoadGame(Payload.ContinueSave, OnLoadGame);
		}

		void OnSettingsClick()
		{
			App.Callbacks.DialogRequest(DialogRequest.ConfirmDeny(LanguageStringModel.Override("Testing sounds."), DialogStyles.Warning));
			//OnNotImplimentedClick();
		}

		void OnCreditsClick()
		{
			OnNotImplimentedClick();
		}

		void OnExitClick()
		{
			Debug.Log("Quiting");
			Application.Quit();
		}

		void OnNotImplimentedClick()
		{
			App.Callbacks.DialogRequest(DialogRequest.Confirm(LanguageStringModel.Override("This feature is not implemented yet."), style: DialogStyles.Warning));
		}
		#endregion

		#region Game Utility
		void StartNewGame()
		{
			App.GameService.CreateGame(Payload.NewGameBlock, OnStartNewGame);
		}

		void OnStartNewGame(RequestResult result, GameModel model)
		{
			if (result.IsNotSuccess)
			{
				Debug.LogError("todo this!");
				return;
			}
			StartGame(model);
		}

		void OnLoadGame(RequestResult result, GameModel model)
		{
			if (result.IsNotSuccess)
			{
				Debug.LogError("todo this!");
				return;
			}
			StartGame(model);
		}

		void StartGame(GameModel model)
		{
			SM.PushBlocking(
				done => App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetNoFocus(), done)),
				"StartGameSetNoFocus"
			);

			SM.PushBlocking(
				done => App.Callbacks.CameraMaskRequest(CameraMaskRequest.Hide(Payload.MenuAnimationMultiplier * CameraMaskRequest.DefaultHideDuration, done)),
				"StartGameHideCamera"
			);

			SM.Push(
				() => 
				{
					Debug.Log("Starting game...");
					var payload = new GamePayload
					{
						MainCamera = Payload.MainCamera,
						Game = model
					};
					App.SM.RequestState(payload);
				},
				"StartGameRequestState"
			);
		}
		#endregion
	}
}