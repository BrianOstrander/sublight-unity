using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Presenters;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class InitializePayload : IStatePayload
	{
		public List<GameObject> DefaultViews = new List<GameObject>();
		public DefaultShaderGlobals ShaderGlobals;

		public HomePayload homePayload = new HomePayload();
	}

	public class InitializeState : State<InitializePayload>
	{
		// Reminder: Keep variables in payload for easy reset of states!

		public override StateMachine.States HandledState { get { return StateMachine.States.Initialize; } }

		protected override void Begin()
		{
			Payload.ShaderGlobals.Apply();
			App.SM.PushBlocking(InitializeModels);
			App.SM.PushBlocking(InitializeViews);
			App.SM.PushBlocking(InitializePresenters);
			App.SM.PushBlocking(InitializePreferences);
			App.SM.PushBlocking(InitializeEncounters);
			App.SM.PushBlocking(InitializeListeners);

			if (DevPrefs.WipeGameSavesOnStart) App.SM.PushBlocking(WipeGameSaves);
		}

		protected override void Idle()
		{
			App.Callbacks.PlayState(PlayState.Playing);

			App.P.AddGlobals(new DialogPresenter());
			App.P.AddGlobals(new ShadePresenter());

			if (DevPrefs.AutoNewGame) App.GameService.CreateGame(OnAutoNewGame);
			else App.SM.RequestState(Payload.homePayload);
		}

		#region Mediators
		void InitializeModels(Action done)
		{
			App.M.Initialize(
				App.BuildPreferences.Info,
				status =>
				{
					if (status == RequestStatus.Success) App.Log("ModelMediator Initialized", LogTypes.Initialization);
					else App.Restart("Initializing ModelMediator failed with status " + status);
					// TODO: Should this be called if a restart happens?
					done();
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
					if (status == RequestStatus.Success) App.Log("ViewMediator Initialized", LogTypes.Initialization);
					else App.Restart("Initializing ViewMediator failed with status " + status);
					// TODO: Should this be called if a restart happens?
					done();
				}
			);
		}

		void InitializePresenters(Action done)
		{
			App.P.Initialize(
				status =>
				{
					if (status == RequestStatus.Success) App.Log("PresenterMediator Initialized", LogTypes.Initialization);
					else App.Restart("Initializing PresenterMediator failed with status " + status);
					// TODO: Should this be called if a restart happens?
					done();
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
					if (status == RequestStatus.Success) App.Log("Encounters Initialized", LogTypes.Initialization);
					else App.Restart("Initializing Encounters failed with status " + status);
					// TODO: Should this be called if a restart happens?
					done();
				}
			);
		}

		void InitializeListeners(Action done)
		{
			App.Callbacks.UniversePositionRequest += UniversePosition.OnUniversePositionRequest;
			done();
		}

		void WipeGameSaves(Action done)
		{
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

		void OnAutoNewGame(RequestStatus result, GameModel model)
		{
			var payload = new GamePayload();
			payload.Game = model;
			App.SM.RequestState(payload);
		}
	}
}