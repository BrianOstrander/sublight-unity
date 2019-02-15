using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Presenters;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class InitializePayload : IStatePayload
	{
		public List<GameObject> DefaultViews = new List<GameObject>();
		public DefaultShaderGlobals ShaderGlobals;

		public HomePayload HomeStatePayload = new HomePayload();
	}

	public class InitializeState : State<InitializePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Initialize; } }

		protected override void Begin()
		{
			Payload.ShaderGlobals.Apply();
			SM.PushBlocking(InitializeModels, "InitializeModels");
			SM.PushBlocking(InitializeViews, "InitializeViews");
			SM.PushBlocking(InitializePresenters, "InitializePresenters");
			SM.PushBlocking(InitializePreferences, "InitializePreferences");
			SM.PushBlocking(InitializeEncounters, "InitializeEncounters");
			SM.PushBlocking(InitializeListeners, "InitializeListeners");
			SM.PushBlocking(InitializeMetaKeyValues, "InitializeMetakeyValues");

			if (DevPrefs.WipeGameSavesOnStart) SM.PushBlocking(WipeGameSaves, "WipeGameSaves");
		}

		protected override void Idle()
		{
			var mainCamera = (Payload.HomeStatePayload.MainCamera = new HoloRoomFocusCameraPresenter());

			App.P.AddGlobals(
				new DialogPresenter(
					LanguageStringModel.Override("Alert"),
					LanguageStringModel.Override("Confirm"),

					LanguageStringModel.Override("Okay"),
					LanguageStringModel.Override("Yes"),
					LanguageStringModel.Override("No"),
					LanguageStringModel.Override("Cancel")
				),
				mainCamera,
				new GenericFocusCameraPresenter<PriorityFocusDetails>(mainCamera.GantryAnchor, mainCamera.FieldOfView)
			);

			switch (DevPrefs.AutoGameOption.Value)
			{
				case AutoGameOptions.None:
					break;
				case AutoGameOptions.NewGame:
					App.GameService.CreateGame(DevPrefs.DevCreateGame, OnAutoNewGame);
					return;
				case AutoGameOptions.ContinueGame:
					App.GameService.ContinueGame(OnAutoContinueGame);
					return;
				default:
					Debug.Log("Unrecognized AutoGameOption: " + DevPrefs.AutoGameOption.Value+ ", fallbing back to AutoGameOptions.None behaviour");
					break;
			}

			OnRequestHomeState();
		}

		#region Mediators
		void InitializeModels(Action done)
		{
			App.M.Initialize(
				App.BuildPreferences.Info,
				status =>
				{
					if (status == RequestStatus.Success)
					{
						App.Log("ModelMediator Initialized", LogTypes.Initialization);
						done();
					}
					else App.Restart("Initializing ModelMediator failed with status " + status);
				}
			);
		}

		void InitializeViews(Action done)
		{
			App.V.Initialize(
				Payload.DefaultViews,
				App.Main.transform,
				status =>
				{
					if (status == RequestStatus.Success)
					{
						App.Log("ViewMediator Initialized", LogTypes.Initialization);
						done();
					}
					else App.Restart("Initializing ViewMediator failed with status " + status);
				}
			);
		}

		void InitializePresenters(Action done)
		{
			App.P.Initialize(
				status =>
				{
					if (status == RequestStatus.Success)
					{
						App.Log("PresenterMediator Initialized", LogTypes.Initialization);
						done();
					}
					else App.Restart("Initializing PresenterMediator failed with status " + status);
				}
			);
		}
		#endregion

		#region Preferences
		void InitializePreferences(Action done)
		{
			App.M.List<PreferencesModel>(result => OnListPreferences(result, done));
		}

		void OnListPreferences(SaveLoadArrayRequest<SaveModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				App.Restart("Listing preferences failed with status" + result.Status);
				return;
			}

			if (result.Length == 0)
			{
				App.Log("No existing preferences, generating defaults", LogTypes.Initialization);
				App.M.Save(
					App.M.Create<PreferencesModel>(), 
					saveResult => OnSavedPreferences(saveResult, done)
				);
			}
			else
			{
				var toLoad = result.Models.Where(p => p.SupportedVersion.Value).OrderBy(p => p.Version.Value).LastOrDefault();
				if (toLoad == null)
				{
					App.Log("No supported preferences, generating defaults", LogTypes.Initialization);
					App.M.Save(
						App.M.Create<PreferencesModel>(),
						saveResult => OnSavedPreferences(saveResult, done)
					);
				}
				else
				{
					App.Log("Loading existing preferences", LogTypes.Initialization);
					App.M.Load<PreferencesModel>(
						toLoad,
						loadResult => OnLoadPreferences(loadResult, done)
					);
				}
			}
		}

		void OnLoadPreferences(SaveLoadRequest<PreferencesModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				App.Restart("Loading preferences failed with status" + result.Status);
				return;
			}

			App.Log("Loaded preferences from "+result.Model.Path, LogTypes.Initialization);
			App.M.Save(
				result.TypedModel,
				saveResult => OnSavedPreferences(saveResult, done)
			);
		}

		void OnSavedPreferences(SaveLoadRequest<PreferencesModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				App.Restart("Saving preferences failed with status" + result.Status);
				return;
			}

			App.Log("Saved preferences to " + result.Model.Path, LogTypes.Initialization);
			App.SetPreferences(result.TypedModel);
			done();
		}
		#endregion

		void InitializeEncounters(Action done)
		{
			App.Encounters.Initialize(
				status =>
				{
					if (status == RequestStatus.Success)
					{
						App.Log("Encounters Initialized", LogTypes.Initialization);
						done();
					}
					else App.Restart("Initializing Encounters failed with status " + status);
				}
			);
		}

		void InitializeListeners(Action done)
		{
			done();
		}

		void InitializeMetaKeyValues(Action done)
		{
			App.MetaKeyValues.Initialize(
				status =>
				{
					if (status == RequestStatus.Success)
					{
						App.Log("Meta KVs Initialized", LogTypes.Initialization);
						done();
					}
					else App.Restart("Initializing Meta KVs failed with status " + status);
				}
			);
		}

		void WipeGameSaves(Action done)
		{
			switch (DevPrefs.AutoGameOption.Value)
			{
				case AutoGameOptions.ContinueGame:
					Debug.LogWarning("Auto Continue Game behaviour is selected, canceling wipe save action");
					done();
					return;
			}

			App.M.List<GameModel>(result => OnWipeGameSavesLoad(result, done));
		}

		void OnWipeGameSavesLoad(SaveLoadArrayRequest<SaveModel> result, Action done)
		{
			OnWipeGameSavesDelete(RequestStatus.Success, default(SaveLoadRequest<SaveModel>), result.Models.ToList(), done);
		}

		void OnWipeGameSavesDelete(RequestStatus status, SaveLoadRequest<SaveModel> result, List<SaveModel> remaining, Action done)
		{
			if (remaining.Count == 0)
			{
				done();
				return;
			}
			if (status != RequestStatus.Success) Debug.LogError("Unable to delete, returned error: " + result.Error);
			var current = remaining[0];
			remaining.RemoveAt(0);
			App.M.Delete(current, deleteResult => OnWipeGameSavesDelete(deleteResult.Status, deleteResult, remaining, done));
		}

		void OnRequestHomeState()
		{
			App.SM.RequestState(Payload.HomeStatePayload);
		}

		void OnAutoNewGame(RequestResult result, GameModel model)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Creating game returned status "+result.Status+", falling back to AutoGameOptions.None behaviour");
				OnRequestHomeState();
				return;
			}

			Debug.Log("Auto New Game...");
			OnRequestGameState(model);
		}

		void OnAutoContinueGame(RequestResult result, GameModel model)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Unable to load a continuable game, falling back to AutoGameOptions.None behaviour");
				OnRequestHomeState();
				return;
			}

			if (model == null)
			{
				Debug.LogWarning("No continue save to load, falling back to AutoGameOptions.None behaviour");
				OnRequestHomeState();
				return;
			}

			Debug.Log("Auto Continue Game...");
			OnRequestGameState(model);
		}

		void OnRequestGameState(GameModel model)
		{
			var payload = new GamePayload();
			payload.MainCamera = Payload.HomeStatePayload.MainCamera;
			payload.Game = model;
			App.SM.RequestState(payload);
		}
	}
}