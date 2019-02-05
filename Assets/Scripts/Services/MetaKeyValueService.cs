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
		ILogService logger;

		public GlobalKeyValuesModel GlobalKeyValues { get; private set; }
		public PreferencesKeyValuesModel PreferencesKeyValues { get; private set; }
		bool currentlySaving;

		public MetaKeyValueService(
			CallbackService callbacks,
			IModelMediator modelMediator,
			KeyValueService keyValues,
			ILogService logger
		)
		{
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (modelMediator == null) throw new ArgumentNullException("modelMediator");
			if (keyValues == null) throw new ArgumentNullException("keyValues");
			if (logger == null) throw new ArgumentNullException("logger");

			this.callbacks = callbacks;
			this.modelMediator = modelMediator;
			this.keyValues = keyValues;
			this.logger = logger;
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
					preferencesResult => OnInitializeDone(preferencesResult, done)
				)
			);
		}

		void OnInitializeList<T>(SaveLoadArrayRequest<SaveModel> result, Action<RequestStatus> done)
			where T : SaveModel, new()
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Listing " + typeof(T).Name + " failed with status " + result.Status+"\nError: "+result.Error);
				done(result.Status);
				return;
			}

			if (result.Length == 0)
			{
				logger.Log("No existing " + typeof(T).Name + ", generating defaults", LogTypes.Initialization);
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
					logger.Log("No supported " + typeof(T).Name + ", generating defaults", LogTypes.Initialization);
					modelMediator.Save(
						modelMediator.Create<T>(),
						saveResult => OnInitializedSaved(saveResult, done)
					);
				}
				else
				{
					logger.Log("Loading existing " + typeof(T).Name, LogTypes.Initialization);
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
				Debug.LogError("Loading " + typeof(T).Name + " failed with status " + result.Status+ "\nError: " + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Loaded " + typeof(T).Name + " from " + result.Model.Path, LogTypes.Initialization);
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
				Debug.LogError("Saving " + typeof(T).Name + " failed with status " + result.Status+ "\nError: " + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Saved " + typeof(T).Name + " " + result.Model.Path, LogTypes.Initialization);

			switch (result.Model.SaveType)
			{
				case SaveTypes.GlobalKeyValues:
					GlobalKeyValues = result.TypedModel as GlobalKeyValuesModel;
					break;
				case SaveTypes.PreferencesKeyValues:
					PreferencesKeyValues = result.TypedModel as PreferencesKeyValuesModel;
					break;
			}

			done(RequestStatus.Success);
		}

		void OnInitializeDone(RequestStatus result, Action<RequestStatus> done)
		{
			if (result != RequestStatus.Success)
			{
				Debug.LogError("Initializing preferences key values failed with status " + result);
				done(result);
				return;
			}

			currentlySaving = false;

			callbacks.StateChange += OnStateChange;
			callbacks.SaveRequest += OnSaveRequest;

			// Keyvalue listeners are created here and just float about... never being unregistered... so keep that in mind I guess...
			new KeyValueListener(KeyValueTargets.Global, GlobalKeyValues.KeyValues, keyValues).Register();
			new KeyValueListener(KeyValueTargets.Preferences, PreferencesKeyValues.KeyValues, keyValues).Register();

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

		void OnTrySave()
		{
			if (currentlySaving)
			{
				Debug.LogWarning("Currently trying to save meta key values.");
				return;
			}
			currentlySaving = true;

			modelMediator.Save(GlobalKeyValues, OnTrySaveGlobals);
		}

		void OnTrySaveGlobals(SaveLoadRequest<GlobalKeyValuesModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Trying to save global key values failed with status " + result.Status + "\nError: " + result.Error);
			}

			modelMediator.Save(PreferencesKeyValues, OnTrySavePreferences);
		}

		void OnTrySavePreferences(SaveLoadRequest<PreferencesKeyValuesModel> result)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Trying to save preferences key values failed with status " + result.Status+ "\nError: " + result.Error);
			}

			OnTrySaveDone();
		}

		void OnTrySaveDone()
		{
			currentlySaving = false;
		}
		#endregion
	}
}