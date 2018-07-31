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

		InteractedInventoryReferenceListModel interactedReferences;
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
				modelMediator.List<InteractedInventoryReferenceListModel>(interactableResult => OnListInteractedReferences(interactableResult, done));
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
					modelMediator.List<ModuleReferenceModel>(result => OnListShared<ModuleReferenceModel>(result, null, OnLoadedShared, listDone));
					break;
				case SaveTypes.OrbitalCrewReference:
					modelMediator.List<OrbitalCrewReferenceModel>(result => OnListShared<OrbitalCrewReferenceModel>(result, null, OnLoadedShared, listDone));
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

		void OnLoadedShared<T>(SaveLoadRequest<T> result) where T : SaveModel
		{
			references.All.Value = references.All.Value.Append(result.TypedModel as IInventoryReferenceModel).ToArray();
		}

		void OnListInteractedReferences(SaveLoadArrayRequest<SaveModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Listing interacted references failed with status " + result.Status + " and error:\n" + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Loaded " + references.All.Value.Length + " references", LogTypes.Initialization);

			if (result.Length == 0)
			{
				logger.Log("No existing interacted references, generating defaults", LogTypes.Initialization);
				modelMediator.Save(
					UpdateInteractedReferences(references.All.Value, modelMediator.Create<InteractedInventoryReferenceListModel>()),
					saveResult => OnSavedInteractedReferences(saveResult, done)
				);
			}
			else
			{
				var toLoad = result.Models.Where(p => p.SupportedVersion.Value).OrderBy(p => p.Version.Value).LastOrDefault();
				if (toLoad == null)
				{
					logger.Log("No supported interacted references, generating defaults", LogTypes.Initialization);
					modelMediator.Save(
						UpdateInteractedReferences(references.All.Value, modelMediator.Create<InteractedInventoryReferenceListModel>()),
						saveResult => OnSavedInteractedReferences(saveResult, done)
					);
				}
				else
				{
					logger.Log("Loading existing interacted references", LogTypes.Initialization);
					modelMediator.Load<InteractedInventoryReferenceListModel>(
						toLoad,
						loadResult => OnLoadInteractedReferences(loadResult, done)
					);
				}
			}
		}

		void OnLoadInteractedReferences(SaveLoadRequest<InteractedInventoryReferenceListModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Loading interacted references failed with status " + result.Status + " and error:\n" + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Loaded interacted references from " + result.Model.Path, LogTypes.Initialization);
			modelMediator.Save(
				UpdateInteractedReferences(references.All.Value, result.TypedModel),
				saveResult => OnSavedInteractedReferences(saveResult, done)
			);
		}

		void OnSavedInteractedReferences(SaveLoadRequest<InteractedInventoryReferenceListModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Saving interacted references failed with status " + result.Status + " and error:\n" + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Saved interacted references to " + result.Model.Path, LogTypes.Initialization);

			currentlySaving = false;
			interactedReferences = result.TypedModel;
			callbacks.SaveRequest += OnSaveRequest;

			done(RequestStatus.Success);
		}

		InteractedInventoryReferenceListModel UpdateInteractedReferences(IInventoryReferenceModel[] allReferences, InteractedInventoryReferenceListModel target)
		{
			var allReferenceIds = allReferences.Select(e => e.RawModel.InventoryId.Value);
			var existingTargets = target.References.Value.Where(e => allReferenceIds.Contains(e.InventoryId));
			var newTargets = new List<InteractedInventoryReferenceModel>();

			foreach (var reference in allReferences)
			{
				var entry = existingTargets.FirstOrDefault(t => t.InventoryId.Value == reference.RawModel.InventoryId.Value);
				if (entry == null) continue;
				newTargets.Add(entry);
			}

			target.References.Value = newTargets.ToArray();

			return target;
		}
		#endregion

		#region Utility

		#endregion

		#region Events
		void OnSaveRequest(SaveRequest request)
		{
			if (request.State == SaveRequest.States.Complete) OnTrySave();
		}

		void OnTrySave()
		{
			if (currentlySaving) return;
			modelMediator.Save(interactedReferences, OnTrySaved);
		}

		void OnTrySaved(SaveLoadRequest<InteractedInventoryReferenceListModel> result)
		{
			currentlySaving = false;

			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Trying to save interacted references list failed with status " + result.Status + "\nError: " + result.Error);
			}
		}
		#endregion
	}
}