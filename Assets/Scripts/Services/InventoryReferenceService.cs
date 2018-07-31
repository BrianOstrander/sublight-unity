using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class InventoryReferenceService
	{
		IModelMediator modelMediator;
		ILogService logger;
		CallbackService callbacks;

		InventoryReferenceListModel references = new InventoryReferenceListModel();

		InteractedEncounterInfoListModel interactedEncounters;
		bool currentlySaving;

		public InventoryReferenceService(IModelMediator modelMediator, ILogService logger, CallbackService callbacks)
		{
			if (modelMediator == null) throw new ArgumentNullException("modelMediator");
			if (logger == null) throw new ArgumentNullException("logger");
			if (callbacks == null) throw new ArgumentNullException("callbacks");

			this.modelMediator = modelMediator;
			this.logger = logger;
			this.callbacks = callbacks;
		}

		#region Initialization
		public void Initialize(Action<RequestStatus> done)
		{
			currentlySaving = true;

			ListReferences(SaveTypeValidator.InventoryReferences.Where(t => t != SaveTypes.Unknown).ToList(), done);
		}

		void ListReferences(List<SaveTypes> remainingTypes, Action<RequestStatus> done)
		{
			if (remainingTypes.Count == 0)
			{
				logger.Log("Loaded " + references.All.Value.Length + " references", LogTypes.Initialization);
				Debug.LogWarning("todo: move on to interacted references stuff!");
				done(RequestStatus.Success);
				return;
			}

			var nextType = remainingTypes[0];
			remainingTypes.RemoveAt(0);

			Action<RequestStatus, string> listDone = (listResult, listError) =>
			{
				if (listResult != RequestStatus.Success)
				{
					Debug.LogError("Listing references failed with status: " + listResult + " and error:\n" + listError);
					done(listResult);
					return;
				}

				ListReferences(remainingTypes, done);
			};

			switch(nextType)
			{
				case SaveTypes.ModuleReference:
					modelMediator.List<ModuleReferenceModel>(result => OnListShared<ModuleReferenceModel>(result, null, OnLoadModule, listDone));
					break;
				case SaveTypes.OrbitalCrewReference:
					modelMediator.List<OrbitalCrewReferenceModel>(result => OnListShared<OrbitalCrewReferenceModel>(result, null, OnLoadOrbitalCrew, listDone));
					break;
				default:
					Debug.LogError("Unrecognized SaveType: "+nextType);
					done(RequestStatus.Failure);
					break;
			}
		}

		void OnListShared<T>(
			SaveLoadArrayRequest<SaveModel> result, 
			List<SaveModel> remaining,
			Action<SaveLoadRequest<T>> loadReference,
			Action<RequestStatus, string> listDone
		)
			where T : SaveModel
		{
			if (result.Status != RequestStatus.Success)
			{
				listDone(result.Status, result.Error);
				return;
			}

			remaining = remaining ?? result.Models.ToList();

			if (remaining.Count == 0)
			{
				listDone(RequestStatus.Success, null);
				return;
			}

			var nextReference = remaining[0];
			remaining.RemoveAt(0);

			Action<RequestStatus, string> loadDone = (loadResult, loadError) =>
			{
				if (loadResult != RequestStatus.Success)
				{
					listDone(loadResult, loadError);
					return;
				}

				OnListShared(result, remaining, loadReference, listDone);
			};

			modelMediator.Load<T>(
				nextReference, 
				loadReferenceResult => OnLoadShared(loadReferenceResult, loadDone, loadReference)
			);
		}

		void OnLoadShared<T>(
			SaveLoadRequest<T> result, 
			Action<RequestStatus, string> loadDone, 
			Action<SaveLoadRequest<T>> onLoad
		)
			where T : SaveModel
		{
			if (result.Status == RequestStatus.Success)
			{
				var hasCalledBack = false;
				try
				{
					// To keep things simple, any of these actions should just throw exceptions if they have issues.
					onLoad(result);
				}
				catch (Exception e)
				{
					loadDone(RequestStatus.Failure, e.Message);
					hasCalledBack = true;
				}
				if (!hasCalledBack) loadDone(RequestStatus.Success, null);
			}
			else loadDone(result.Status, result.Error);
		}
		
		void OnLoadModule(SaveLoadRequest<ModuleReferenceModel> result)
		{
			references.All.Value = references.All.Value.Append(result.TypedModel).ToArray();
		}

		void OnLoadOrbitalCrew(SaveLoadRequest<OrbitalCrewReferenceModel> result)
		{
			references.All.Value = references.All.Value.Append(result.TypedModel).ToArray();
		}

		#endregion
	}
}