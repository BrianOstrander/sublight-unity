using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct InventoryReferenceRequest<M> where M : InventoryModel
	{
		public RequestStatus Status { get; private set; }
		public IInventoryReferenceModel Reference { get; private set; }
		public M Instance { get; private set; }
		public string Error { get; private set; }

		public static InventoryReferenceRequest<M> Success(IInventoryReferenceModel reference, M instance)
		{
			return new InventoryReferenceRequest<M>(
				RequestStatus.Success,
				reference,
				instance
			);
		}

		public static InventoryReferenceRequest<M> Failure(IInventoryReferenceModel reference, M instance, string error)
		{
			return new InventoryReferenceRequest<M>(
				RequestStatus.Failure,
				reference,
				instance,
				error
			);
		}

		InventoryReferenceRequest(
			RequestStatus status,
			IInventoryReferenceModel reference,
			M instance,
			string error = null
		)
		{
			Status = status;
			Reference = reference;
			Instance = instance;
			Error = error;
		}
	}

	public class InventoryReferenceService
	{
		IModelMediator modelMediator;
		ILogService logger;
		CallbackService callbacks;
		ValueFilterService filterService;

		InventoryReferenceListModel references = new InventoryReferenceListModel();

		InteractedInventoryReferenceListModel interactedReferences;
		bool currentlySaving;

		public InventoryReferenceService(
			IModelMediator modelMediator,
			ILogService logger,
			CallbackService callbacks,
			ValueFilterService filterService
		)
		{
			if (modelMediator == null) throw new ArgumentNullException("modelMediator");
			if (logger == null) throw new ArgumentNullException("logger");
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (filterService == null) throw new ArgumentNullException("filterService");

			this.modelMediator = modelMediator;
			this.logger = logger;
			this.callbacks = callbacks;
			this.filterService = filterService;
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

			switch (nextType)
			{
				case SaveTypes.ModuleReference:
					modelMediator.List<ModuleReferenceModel>(result => OnListShared<ModuleReferenceModel>(result, null, OnLoadedShared, listDone));
					break;
				case SaveTypes.OrbitalCrewReference:
					modelMediator.List<OrbitalCrewReferenceModel>(result => OnListShared<OrbitalCrewReferenceModel>(result, null, OnLoadedShared, listDone));
					break;
				default:
					Debug.LogError("Unrecognized SaveType: " + nextType);
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
		public void CreateInstance(string inventoryId, InventoryReferenceContext context, Action<InventoryReferenceRequest<InventoryModel>> done)
		{
			CreateInstance<InventoryModel>(inventoryId, context, done);
		}

		public void CreateInstance<M>(string inventoryId, InventoryReferenceContext context, Action<InventoryReferenceRequest<M>> done)
			where M : InventoryModel
		{
			var reference = references.GetReferenceFirstOrDefault(inventoryId);
			if (reference == null)
			{
				var error = "Unable to find a reference with InventoryId " + inventoryId;
				Debug.LogError(error);
				done(InventoryReferenceRequest<M>.Failure(null, null, error));
				return;
			}

			switch (reference.RawModel.InventoryType)
			{
				case InventoryTypes.Module:
					modelMediator.Load<ModuleReferenceModel>(reference as ModuleReferenceModel, result => OnCreateInstanceLoaded(result, context, done));
					break;
				case InventoryTypes.OrbitalCrew:
					modelMediator.Load<OrbitalCrewReferenceModel>(reference as OrbitalCrewReferenceModel, result => OnCreateInstanceLoaded(result, context, done));
					break;
				default:
					var error = "Unrecognized InventoryType: " + reference.RawModel.InventoryType;
					Debug.LogError(error);
					done(InventoryReferenceRequest<M>.Failure(null, null, error));
					break;
			}
		}

		void OnCreateInstanceLoaded<R, M>(SaveLoadRequest<R> result, InventoryReferenceContext context, Action<InventoryReferenceRequest<M>> done)
			where R : SaveModel, IInventoryReferenceModel
			where M : InventoryModel
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError(result.Error);
				done(InventoryReferenceRequest<M>.Failure(null, null, result.Error));
				return;
			}

			var reference = result.TypedModel;
			reference.InitializeInstance(context);
			var instance = reference.RawModel as M;

			done(InventoryReferenceRequest<M>.Success(reference, instance));
		}

		public void CreateRandomInstance(ValueFilterModel filtering, GameModel model, InventoryReferenceContext context, Action<InventoryReferenceRequest<InventoryModel>> done)
		{
			OnCreateRandomInstanceFilterReference(
				null,
				new List<IInventoryReferenceModel>(),
				references.All.Value.ToList(),
				filtering,
				model,
				context,
				results => OnCreateRandomInstanceFilterReferenceDone(results, context, done)
			);
		}

		void OnCreateRandomInstanceFilterReference(
			IInventoryReferenceModel result,
			List<IInventoryReferenceModel> filtered,
			List<IInventoryReferenceModel> remaining,
			ValueFilterModel filtering,
			GameModel model,
			InventoryReferenceContext context,
			Action<IInventoryReferenceModel[]> done
		)
		{
			if (result != null) filtered.Add(result);

			if (remaining.None())
			{
				done(filtered.ToArray());
				return;
			}

			var next = remaining.First();
			remaining.RemoveAt(0);

			Action<bool> filterDone = filterResult =>
			{
				OnCreateRandomInstanceFilterReference(
					filterResult ? next : null,
					filtered,
					remaining,
					filtering,
					model,
					context,
					done
				);
			};

			filterService.Filter(
				filterDone,
				filtering,
				model,
				next.RawModel
			);
		}

		void OnCreateRandomInstanceFilterReferenceDone(
			IInventoryReferenceModel[] results,
			InventoryReferenceContext context,
			Action<InventoryReferenceRequest<InventoryModel>> done
		)
		{
			if (results.None())
			{
				var error = "No inventory references pass that filtering.";
				Debug.LogError(error);
				done(InventoryReferenceRequest<InventoryModel>.Failure(null, null, error));
				return;
			}

			var maxRandomWeight = 0f;
			IInventoryReferenceModel chosen = null;

			foreach (var current in results)
			{
				var currentWeight = current.RawModel.RandomWeightMultiplier.Value * DemonUtility.NextFloat;
				if (chosen == null || maxRandomWeight < currentWeight)
				{
					maxRandomWeight = currentWeight;
					chosen = current;
				}
			}

			CreateInstance(chosen.RawModel.InventoryId.Value, context, done);
		}

		public InteractedInventoryReferenceModel GetReferenceInteraction(string inventoryId)
		{
			return interactedReferences.GetReference(inventoryId);
		}
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