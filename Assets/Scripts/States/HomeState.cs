using System;
using System.Linq;
using System.Collections.Generic;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Presenters;

using UnityEngine;

namespace LunraGames.SubLight
{
	public class HomePayload : IStatePayload 
	{
		public bool CanContinueSave { get { return ContinueSave != null; } }
		public SaveModel[] Saves = new SaveModel[0];
		public SaveModel ContinueSave;

		public GameObject HoloSurfaceOrigin;

		public float MenuAnimationMultiplier;
		public Dictionary<float, IPresenterCloseShowOptions[]> DelayedPresenterShows = new Dictionary<float, IPresenterCloseShowOptions[]>();

		public GalaxyPreviewModel PreviewGalaxy;
	}

	public partial class HomeState : State<HomePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Home; } }

		static string[] Scenes { get { return new string[] { SceneConstants.Home, SceneConstants.HoloRoom }; } }
		static string[] Tags { get { return new string[] { TagConstants.HoloSurfaceOrigin }; } }

		#region Begin
		protected override void Begin()
		{
			Payload.MenuAnimationMultiplier = DevPrefs.SkipMainMenuAnimations ? 0f : 1f;

			App.SM.PushBlocking(LoadScenes);
			App.SM.PushBlocking(InitializeInput);
			App.SM.PushBlocking(InitializeCallbacks);
			App.SM.PushBlocking(InitializeLoadSaves);
			App.SM.PushBlocking(InitializeLoadGalaxy);
			App.SM.PushBlocking(done => Focuses.InitializePresenters(this, done));
			App.SM.PushBlocking(InitializeFocus);
		}

		void LoadScenes(Action done)
		{
			App.Scenes.Request(SceneRequest.Load(result => OnLoadScenes(result, done), Scenes, Tags));
		}

		void OnLoadScenes(SceneRequest result, Action done)
		{
			foreach (var kv in result.FoundTags)
			{
				switch (kv.Key)
				{
					case TagConstants.HoloSurfaceOrigin: Payload.HoloSurfaceOrigin = kv.Value; break;
				}
			}

			done();
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

		#endregion

		#region Idle
		protected override void Idle()
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

			App.Callbacks.ClearEscapables();

			App.SM.PushBlocking(UnBind);
			App.SM.PushBlocking(UnLoadScenes);
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
			if (Payload.CanContinueSave) App.Callbacks.DialogRequest(DialogRequest.CancelConfirm(LanguageStringModel.Override("Starting a new game will overwrite your existing one."), DialogStyles.Warning, LanguageStringModel.Override("Overwrite Game"), confirm: OnNewGameStart));
			else OnNewGameStart();
		}

		void OnNewGameStart()
		{
			App.GameService.CreateGame(OnNewGameCreated);
		}

		void OnContinueGameClick()
		{
			App.M.Load<GameModel>(Payload.ContinueSave, OnLoadedGame);
		}

		void OnSettingsClick()
		{
			OnNotImplimentedClick();
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
			App.Callbacks.DialogRequest(DialogRequest.Alert(LanguageStringModel.Override("This feature is not implemented yet."), style: DialogStyles.Warning));
		}

		void OnNewGameCreated(RequestStatus result, GameModel model)
		{
			if (result != RequestStatus.Success)
			{
				App.Callbacks.DialogRequest(DialogRequest.Alert(LanguageStringModel.Override("Creating new game returned with result " + result), style: DialogStyles.Error));
				return;
			}
			App.M.Save(model, OnSaveGame);
		}

		void OnLoadedGame(SaveLoadRequest<GameModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				App.Callbacks.DialogRequest(DialogRequest.Alert(LanguageStringModel.Override(result.Error), style: DialogStyles.Error));
				return;
			}
			App.M.Save(result.TypedModel, OnSaveGame);
		}

		void OnSaveGame(SaveLoadRequest<GameModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				App.Callbacks.DialogRequest(DialogRequest.Alert(LanguageStringModel.Override(result.Error), style: DialogStyles.Error));
				return;
			}
			OnStartGame(result.TypedModel);
		}

		void OnStartGame(GameModel model)
		{
			var payload = new GamePayload();
			payload.Game = model;
			App.SM.RequestState(payload);
		}
		#endregion
	}
}