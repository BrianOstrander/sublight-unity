using System;
using System.Linq;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class MetaKeyValueService
	{
		CallbackService callbacks;
		IModelMediator modelMediator;
		KeyValueService keyValues;

		GlobalKeyValuesModel globals;
		PreferencesKeyValuesModel preferences;

		public KeyValueListModel GlobalKeyValues { get { return globals.KeyValues; } }
		public KeyValueListModel PreferencesKeyValues { get { return preferences.KeyValues; } }
		bool currentlySaving;

		public MetaKeyValueService(
			CallbackService callbacks,
			IModelMediator modelMediator,
			KeyValueService keyValues
		)
		{
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (modelMediator == null) throw new ArgumentNullException("modelMediator");
			if (keyValues == null) throw new ArgumentNullException("keyValues");

			this.callbacks = callbacks;
			this.modelMediator = modelMediator;
			this.keyValues = keyValues;
		}

		#region Initialize
		public void Initialize(Action<RequestStatus> done)
		{
			currentlySaving = true;
			OnInitializeGlobals(done);
		}

		void OnInitializeGlobals(Action<RequestStatus> done)
		{
			modelMediator.List<GlobalKeyValuesModel>(
				result => OnInitializeList<GlobalKeyValuesModel>(
					result,
					globalResult => OnInitializePreferences(globalResult, done)
				)
			);
		}

		void OnInitializePreferences(RequestStatus result, Action<RequestStatus> done)
		{
			if (result != RequestStatus.Success)
			{
				Debug.LogError("Initializing global key values failed with status " + result);
				done(result);
				return;
			}

			modelMediator.List<PreferencesKeyValuesModel>(
				listResult => OnInitializeList<PreferencesKeyValuesModel>(
					listResult,
					preferencesResult => OnInitializeKeyValues(preferencesResult, done)
				)
			);
		}

		void OnInitializeList<T>(SaveLoadArrayRequest<SaveModel> result, Action<RequestStatus> done)
			where T : SaveModel, new()
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Listing " + typeof(T).Name + " failed with status " + result.Status + "\nError: " + result.Error);
				done(result.Status);
				return;
			}

			if (result.Length == 0)
			{
				if (DevPrefs.LoggingInitialization) Debug.Log("No existing " + typeof(T).Name + ", generating defaults");
				modelMediator.Save(
					modelMediator.Create<T>(),
					saveResult => OnInitializedSaved(saveResult, done)
				);
			}
			else
			{
				var toLoad = result.Models.Where(p => p.SupportedVersion.Value).OrderBy(p => p.Version.Value).LastOrDefault();
				if (toLoad == null)
				{
					if (DevPrefs.LoggingInitialization) Debug.Log("No supported " + typeof(T).Name + ", generating defaults");
					modelMediator.Save(
						modelMediator.Create<T>(),
						saveResult => OnInitializedSaved(saveResult, done)
					);
				}
				else
				{
					if (DevPrefs.LoggingInitialization) Debug.Log("Loading existing " + typeof(T).Name);
					modelMediator.Load<T>(
						toLoad,
						loadResult => OnInitializeLoad(loadResult, done)
					);
				}
			}
		}

		void OnInitializeLoad<T>(SaveLoadRequest<T> result, Action<RequestStatus> done)
			where T : SaveModel, new()
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Loading " + typeof(T).Name + " failed with status " + result.Status + "\nError: " + result.Error);
				done(result.Status);
				return;
			}

			if (DevPrefs.LoggingInitialization) Debug.Log("Loaded " + typeof(T).Name + " from " + result.Model.Path);
			modelMediator.Save(
				result.TypedModel,
				saveResult => OnInitializedSaved(saveResult, done)
			);
		}

		void OnInitializedSaved<T>(SaveLoadRequest<T> result, Action<RequestStatus> done)
			where T : SaveModel, new()
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Saving " + typeof(T).Name + " failed with status " + result.Status + "\nError: " + result.Error);
				done(result.Status);
				return;
			}

			if (DevPrefs.LoggingInitialization) Debug.Log("Saved " + typeof(T).Name + " " + result.Model.Path);

			switch (result.Model.SaveType)
			{
				case SaveTypes.GlobalKeyValues:
					globals = result.TypedModel as GlobalKeyValuesModel;
					break;
				case SaveTypes.PreferencesKeyValues:
					preferences = result.TypedModel as PreferencesKeyValuesModel;
					break;
			}

			done(RequestStatus.Success);
		}

		void OnInitializeKeyValues(RequestStatus result, Action<RequestStatus> done)
		{
			if (result != RequestStatus.Success)
			{
				Debug.LogError("Initializing preferences key values failed with status " + result);
				done(result);
				return;
			}

			if (GlobalKeyValues == null) Debug.LogError("GlobalKeyValues null, skipping...");
			else
			{
				if (string.IsNullOrEmpty(GlobalKeyValues.Get(KeyDefines.Global.PersistentId)))
				{
					Debug.Log("Generating Persistent Id");
					GlobalKeyValues.Set(KeyDefines.Global.PersistentId, Guid.NewGuid().ToString());
				}
			}

			if (PreferencesKeyValues == null) Debug.LogError("PreferencesKeyValues null, skipping...");
			else
			{

			}

			modelMediator.Save(
				globals,
				globalSaveResult =>
				{
					if (globalSaveResult.Status != RequestStatus.Success) Debug.LogError("Saving GlobalKeyValues failed with status " + globalSaveResult.Status + " and error: " + globalSaveResult.Error + "\nTrying to continue...");
					modelMediator.Save(
						preferences,
						preferencesSaveResult => OnInitializeDone(preferencesSaveResult.Status, done)
					);
				}
			);
		}

		void OnInitializeDone(RequestStatus result, Action<RequestStatus> done)
		{
			if (result != RequestStatus.Success)
			{
				Debug.LogError("Initializing key values failed with status " + result);
				done(result);
				return;
			}

			currentlySaving = false;

			callbacks.StateChange += OnStateChange;
			callbacks.SaveRequest += OnSaveRequest;

			// Keyvalue listeners are created here and just float about... never being unregistered... so keep that in mind I guess...
			new KeyValueListener(KeyValueTargets.Global, GlobalKeyValues, keyValues).Register();
			new KeyValueListener(KeyValueTargets.Preferences, PreferencesKeyValues, keyValues).Register();

			done(result);
		}

		#endregion

		#region Events
		void OnStateChange(StateChange stateChange)
		{
			if (stateChange.State == StateMachine.States.Home && stateChange.Event == StateMachine.Events.Begin) OnTrySave();
		}

		void OnSaveRequest(SaveRequest request)
		{
			if (request.State == SaveRequest.States.Complete) OnTrySave();
		}

		void OnTrySave(Action<RequestResult> done = null)
		{
			if (currentlySaving)
			{
				var error = "Currently trying to save meta key values.";
				Debug.LogError(error);
				if (done != null) done(RequestResult.Failure(error));
				return;
			}
			currentlySaving = true;

			modelMediator.Save(globals, globalsResult => OnTrySaveGlobals(globalsResult, done));
		}

		void OnTrySaveGlobals(SaveLoadRequest<GlobalKeyValuesModel> result, Action<RequestResult> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				OnTrySaveDone(
					result.Status,
					"Trying to save global key values failed with status " + result.Status + "\nError: " + result.Error,
					done
				);
				return;
			}

			modelMediator.Save(preferences, preferencesResult => OnTrySavePreferences(preferencesResult, done));
		}

		void OnTrySavePreferences(SaveLoadRequest<PreferencesKeyValuesModel> result, Action<RequestResult> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				OnTrySaveDone(
					result.Status,
					"Trying to save preferences key values failed with status " + result.Status + "\nError: " + result.Error,
					done
				);
				return;
			}

			OnTrySaveDone(
				result.Status,
				null,
				done
			);
		}

		void OnTrySaveDone(
			RequestStatus result,
			string error,
			Action<RequestResult> done
		)
		{
			currentlySaving = false;

			if (result != RequestStatus.Success)
			{
				if (string.IsNullOrEmpty(error)) Debug.Log("Trying to save failed with status: " + result + " and no error specified.");
				else Debug.LogError(error);
			}

			if (done != null) done(new RequestResult(result, error));
		}
		#endregion

		#region Utility
		public bool Get(KeyDefinitions.Boolean key, bool fallback = false) { return GetKeyValueList(key).GetBoolean(key.Key, fallback); }
		public int Get(KeyDefinitions.Integer key, int fallback = 0) { return GetKeyValueList(key).GetInteger(key.Key, fallback); }
		public string Get(KeyDefinitions.String key, string fallback = null) { return GetKeyValueList(key).GetString(key.Key, fallback); }
		public float Get(KeyDefinitions.Float key, float fallback = 0f) { return GetKeyValueList(key).GetFloat(key.Key, fallback); }

		public bool Set(KeyDefinitions.Boolean key, bool value) { return GetKeyValueList(key).SetBoolean(key.Key, value); }
		public int Set(KeyDefinitions.Integer key, int value) { return GetKeyValueList(key).SetInteger(key.Key, value); }
		public string Set(KeyDefinitions.String key, string value) { return GetKeyValueList(key).SetString(key.Key, value); }
		public float Set(KeyDefinitions.Float key, float value) { return GetKeyValueList(key).SetFloat(key.Key, value); }

		KeyValueListModel GetKeyValueList(IKeyDefinition key)
		{
			switch (key.Target)
			{
				case KeyValueTargets.Global: return GlobalKeyValues;
				case KeyValueTargets.Preferences: return PreferencesKeyValues;
				default:
					throw new ArgumentOutOfRangeException("key.Target", "Only Global and Preferences kv stores can be accessed, " + key.Target + " not recognized");
			}
		}

		public void Save(Action<RequestResult> done = null)
		{
			OnTrySave(done);
		}

		#endregion
	}
}