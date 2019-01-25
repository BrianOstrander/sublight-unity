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
		public HoloRoomFocusCameraPresenter MainCamera;

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

			PushBlocking(LoadScenes, "LoadScenes");
			PushBlocking(InitializeInput, "InitializeInput");
			PushBlocking(InitializeCallbacks, "InitializeCallbacks");
			PushBlocking(InitializeLoadSaves, "InitializeLoadSaves");
			PushBlocking(InitializeLoadGalaxy, "InitializeLoadGalaxy");
			PushBlocking(done => Focuses.InitializePresenters(this, done), "InitializePresenters");
			PushBlocking(InitializeFocus, "InitializeFocus");
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
			//App.Callbacks.HoloColorRequest(new HoloColorRequest(new Color(0.259f, 0.0393f, 0f)));
			App.Callbacks.CameraMaskRequest(CameraMaskRequest.Reveal(Payload.MenuAnimationMultiplier * 0.75f, OnIdleShow));
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

			PushBlocking(UnBind, "UnBind");
			PushBlocking(UnLoadScenes, "UnLoadScenes");
		}

		void UnLoadScenes(Action done)
		{
			App.Scenes.Request(SceneRequest.UnLoad(result => done(), Scenes));
		}

		void UnBind(Action done)
		{
			// All presenters will have their views closed and unbinded. Events
			// will also be unbinded.
			App.P.UnRegisterAll(done);
		}
		#endregion

		#region Events
		void OnDialogRequest(DialogRequest request)
		{
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

		#region Events Main Menu
		void OnNewGameClick()
		{
			if (Payload.CanContinueSave) App.Callbacks.DialogRequest(DialogRequest.ConfirmDeny(LanguageStringModel.Override("Starting a new game will overwrite your existing one."), DialogStyles.Warning, LanguageStringModel.Override("Overwrite Game"), OnNewGameStart));
			else OnNewGameStart();
		}

		void OnNewGameStart()
		{
			App.GameService.CreateGame(Payload.NewGameBlock, OnNewGameCreated);
		}

		void OnContinueGameClick()
		{
			App.M.Load<GameModel>(Payload.ContinueSave, OnLoadedGame);
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

		void OnNewGameCreated(RequestStatus result, GameModel model)
		{
			if (result != RequestStatus.Success)
			{
				App.Callbacks.DialogRequest(DialogRequest.Confirm(LanguageStringModel.Override("Creating new game returned with result " + result), style: DialogStyles.Error));
				return;
			}
			App.M.Save(model, OnSaveGame);
		}

		void OnLoadedGame(SaveLoadRequest<GameModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				App.Callbacks.DialogRequest(DialogRequest.Confirm(LanguageStringModel.Override(result.Error), style: DialogStyles.Error));
				return;
			}
			App.M.Save(result.TypedModel, OnSaveGame);
		}

		void OnSaveGame(SaveLoadRequest<GameModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				App.Callbacks.DialogRequest(DialogRequest.Confirm(LanguageStringModel.Override(result.Error), style: DialogStyles.Error));
				return;
			}
			OnStartGame(result.TypedModel);
		}

		void OnStartGame(GameModel model)
		{
			OnReadyTransition(() => OnFinallyStartGame(model));
		}

		void OnFinallyStartGame(GameModel model)
		{
			Debug.Log("Starting game...");
			var payload = new GamePayload();
			payload.MainCamera = Payload.MainCamera;
			payload.Game = model;
			App.SM.RequestState(payload);
		}

		void OnReadyTransition(Action done)
		{
			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(Focuses.GetNoFocus(), () => OnReadyTransitionNoFocus(done)));
		}

		void OnReadyTransitionNoFocus(Action done)
		{
			App.Callbacks.CameraMaskRequest(CameraMaskRequest.Hide(Payload.MenuAnimationMultiplier * 0.2f, done));
		}
		#endregion
	}
}