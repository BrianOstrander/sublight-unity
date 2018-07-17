using System;
using System.Linq;

using UnityEngine;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class GlobalKeyValueService
	{
		CallbackService callbacks;
		IModelMediator modelMediator;
		KeyValueService keyValues;
		ILogService logger;

		GlobalKeyValuesModel globalKeyValues;
		KeyValueListener keyValueListener;
		bool currentlySaving;

		public GlobalKeyValueService(CallbackService callbacks, IModelMediator modelMediator, KeyValueService keyValues, ILogService logger)
		{
			this.callbacks = callbacks;
			this.modelMediator = modelMediator;
			this.keyValues = keyValues;
			this.logger = logger;
		}

		#region Initialize
		public void Initialize(Action<RequestStatus> done)
		{
			currentlySaving = true;
			modelMediator.List<GlobalKeyValuesModel>(result => OnListGlobals(result, done));
		}

		void OnListGlobals(SaveLoadArrayRequest<SaveModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Listing global kvs failed with status " + result.Status+"\nError: "+result.Error);
				done(result.Status);
				return;
			}

			if (result.Length == 0)
			{
				logger.Log("No existing global kvs, generating defaults", LogTypes.Initialization);
				modelMediator.Save(
					modelMediator.Create<GlobalKeyValuesModel>(),
					saveResult => OnSavedGlobals(saveResult, done)
				);
			}
			else
			{
				var toLoad = result.Models.Where(p => p.SupportedVersion.Value).OrderBy(p => p.Version.Value).LastOrDefault();
				if (toLoad == null)
				{
					logger.Log("No supported global kvs, generating defaults", LogTypes.Initialization);
					modelMediator.Save(
						modelMediator.Create<GlobalKeyValuesModel>(),
						saveResult => OnSavedGlobals(saveResult, done)
					);
				}
				else
				{
					logger.Log("Loading existing global kvs", LogTypes.Initialization);
					modelMediator.Load<GlobalKeyValuesModel>(
						toLoad,
						loadResult => OnLoadGlobals(loadResult, done)
					);
				}
			}
		}

		void OnLoadGlobals(SaveLoadRequest<GlobalKeyValuesModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Loading global kvs failed with status " + result.Status+ "\nError: " + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Loaded global kvs from " + result.Model.Path, LogTypes.Initialization);
			modelMediator.Save(
				result.TypedModel,
				saveResult => OnSavedGlobals(saveResult, done)
			);
		}

		void OnSavedGlobals(SaveLoadRequest<GlobalKeyValuesModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Saving global kvs failed with status " + result.Status+ "\nError: " + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Saved global kvs to " + result.Model.Path, LogTypes.Initialization);

			currentlySaving = false;
			globalKeyValues = result.TypedModel;

			callbacks.StateChange += OnStateChange;
			callbacks.SaveRequest += OnSaveRequest;

			keyValueListener = new KeyValueListener(KeyValueTargets.Global, globalKeyValues.KeyValues, keyValues);
			keyValueListener.Register();

			done(RequestStatus.Success);
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
			if (currentlySaving) return;
			modelMediator.Save(globalKeyValues, OnTrySaved);
		}

		void OnTrySaved(SaveLoadRequest<GlobalKeyValuesModel> result)
		{
			currentlySaving = false;

			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Trying to save global kv failed with status " + result.Status+ "\nError: " + result.Error);
			}
		}
		#endregion
	}
}