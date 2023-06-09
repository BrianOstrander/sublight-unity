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
			SM.PushBlocking(InitializeAnalytics, "InitializeAnalytics");
			SM.PushBlocking(InitializeAudio, "InitializeAudio");
			SM.PushBlocking(InitializeSaveMetaKeys, "InitializeSaveMetaKeys");

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

			Payload.HomeStatePayload.FromInitialization = true;

			App.SM.RequestState(Payload.HomeStatePayload);
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
						if (DevPrefs.LoggingInitialization) Debug.Log("ModelMediator Initialized");
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
						if (DevPrefs.LoggingInitialization) Debug.Log("ViewMediator Initialized");
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
						if (DevPrefs.LoggingInitialization) Debug.Log("PresenterMediator Initialized");
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
			App.M.Index<PreferencesModel>(result => OnInitializePreferencesIndex(result, done));
		}

		void OnInitializePreferencesIndex(ModelIndexResult<SaveModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				App.Restart("Listing preferences failed with status" + result.Status);
				return;
			}

			if (result.Length == 0)
			{
				if (DevPrefs.LoggingInitialization) Debug.Log("No existing preferences, generating defaults");
				App.M.Save(
					App.M.Create<PreferencesModel>(App.M.CreateUniqueId()), 
					saveResult => OnInitializePreferencesSaved(saveResult, done)
				);
			}
			else
			{
				var toLoad = result.Models.Where(p => p.SupportedVersion.Value).OrderBy(p => p.Version.Value).LastOrDefault();
				if (toLoad == null)
				{
					if (DevPrefs.LoggingInitialization) Debug.Log("No supported preferences, generating defaults");
					App.M.Save(
						App.M.Create<PreferencesModel>(App.M.CreateUniqueId()),
						saveResult => OnInitializePreferencesSaved(saveResult, done)
					);
				}
				else
				{
					if (DevPrefs.LoggingInitialization) Debug.Log("Loading existing preferences");
					App.M.Load<PreferencesModel>(
						toLoad,
						loadResult => OnInitializePreferencesLoad(loadResult, done)
					);
				}
			}
		}

		void OnInitializePreferencesLoad(ModelResult<PreferencesModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				App.Restart("Loading preferences failed with status" + result.Status);
				return;
			}

			if (DevPrefs.LoggingInitialization) Debug.Log("Loaded preferences from "+result.Model.Path);
			App.M.Save(
				result.TypedModel,
				saveResult => OnInitializePreferencesSaved(saveResult, done)
			);
		}

		void OnInitializePreferencesSaved(ModelResult<PreferencesModel> result, Action done)
		{
			if (result.Status != RequestStatus.Success)
			{
				App.Restart("Saving preferences failed with status " + result.Status);
				return;
			}

			if (DevPrefs.LoggingInitialization) Debug.Log("Saved preferences to " + result.Model.Path);
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
						if (DevPrefs.LoggingInitialization) Debug.Log("Encounters Initialized");
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
						if (DevPrefs.LoggingInitialization) Debug.Log("Meta KVs Initialized");
						done();
					}
					else App.Restart("Initializing Meta KVs failed with status " + status);
				}
			);
		}

		void InitializeAnalytics(Action done)
		{
			App.Analytics.Initialize(
				status => 
				{
					if (status == RequestStatus.Success)
					{
						if (DevPrefs.LoggingInitialization) Debug.Log("Analytics Initialized");
						done();
					}
					else App.Restart("Initializing Analytics failed with status " + status);
				},
				StateMachine.States.Initialize
			);
		}

		void InitializeAudio(Action done)
		{
			App.Audio.Initialize(
				status =>
				{
					if (status == RequestStatus.Success)
					{
						if (DevPrefs.LoggingInitialization) Debug.Log("Audio Initialized");
						done();
					}
					else App.Restart("Initializing Audio failed with status " + status);
				}
			);
		}

		/// <summary>
		/// We do this incase any other initializations have updated the meta
		/// keys.
		/// </summary>
		/// <param name="done">Done.</param>
		void InitializeSaveMetaKeys(Action done)
		{
			App.MetaKeyValues.Save(
				result =>
				{
					if (result.Status == RequestStatus.Success)
					{
						if (DevPrefs.LoggingInitialization) Debug.Log("Meta Keys Initialized Saved");
						done();
					}
					else App.Restart("Initializing Save Meta Keys Faild with status " + result.Status);
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

			App.M.Index<GameModel>(result => OnWipeGameSavesLoad(result, done));
		}

		void OnWipeGameSavesLoad(ModelIndexResult<SaveModel> result, Action done)
		{
			OnWipeGameSavesDelete(RequestStatus.Success, default(ModelResult<SaveModel>), result.Models.ToList(), done);
		}

		void OnWipeGameSavesDelete(RequestStatus status, ModelResult<SaveModel> result, List<SaveModel> remaining, Action done)
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
	}
}