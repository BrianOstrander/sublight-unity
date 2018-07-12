using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class EncounterService
	{
		IModelMediator modelMediator;
		ILogService logger;

		List<EncounterInfoModel> encounters = new List<EncounterInfoModel>();
		InteractedEncounterInfoListModel interactedEncounters;

		public EncounterService(IModelMediator modelMediator, ILogService logger)
		{
			if (modelMediator == null) throw new ArgumentNullException("saveLoadService");
			if (logger == null) throw new ArgumentNullException("logger");

			this.modelMediator = modelMediator;
			this.logger = logger;
		}

		#region Initialization
		public void Initialize(Action<RequestStatus> done)
		{
			modelMediator.List<EncounterInfoModel>(result => OnListEncounters(result, done));
		}

		void OnListEncounters(SaveLoadArrayRequest<SaveModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Listing encounters failed with status " + result.Status + " and error:\n" + result.Error);
				done(result.Status);
				return;
			}
			OnLoadEncounter(null, default(SaveLoadRequest<EncounterInfoModel>), result.Models.ToList(), done);
		}

		void OnLoadEncounter(RequestStatus? status, SaveLoadRequest<EncounterInfoModel> result, List<SaveModel> remaining, Action<RequestStatus> done)
		{
			if (status.HasValue)
			{
				if (status == RequestStatus.Success) encounters.Add(result.TypedModel);
				else
				{
					Debug.LogError("Loading an encounter failed with status " + result.Status + " and error:\n" + result.Error);
				}
			}

			if (remaining.Count == 0)
			{
				modelMediator.List<InteractedEncounterInfoListModel>(interactableResult => OnListInteractedEncounters(interactableResult, done));
				return;
			}

			var next = remaining[0];
			remaining.RemoveAt(0);
			modelMediator.Load<EncounterInfoModel>(next, loadResult => OnLoadEncounter(loadResult.Status, loadResult, remaining, done));
		}

		void OnListInteractedEncounters(SaveLoadArrayRequest<SaveModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Listing interacted encounters failed with status " + result.Status + " and error:\n" + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Loaded " + encounters.Count + " encounters", LogTypes.Initialization);

			if (result.Length == 0)
			{
				logger.Log("No existing interacted encounters, generating defaults", LogTypes.Initialization);
				modelMediator.Save(
					UpdateInteractedEncounters(encounters, modelMediator.Create<InteractedEncounterInfoListModel>()),
					saveResult => OnSavedInteractedEncounters(saveResult, done)
				);
			}
			else
			{
				var toLoad = result.Models.Where(p => p.SupportedVersion.Value).OrderBy(p => p.Version.Value).LastOrDefault();
				if (toLoad == null)
				{
					logger.Log("No supported interacted encounters, generating defaults", LogTypes.Initialization);
					modelMediator.Save(
						UpdateInteractedEncounters(encounters, modelMediator.Create<InteractedEncounterInfoListModel>()),
						saveResult => OnSavedInteractedEncounters(saveResult, done)
					);
				}
				else
				{
					logger.Log("Loading existing interacted encounters", LogTypes.Initialization);
					modelMediator.Load<InteractedEncounterInfoListModel>(
						toLoad,
						loadResult => OnLoadInteractedEncounters(loadResult, done)
					);
				}
			}
		}

		void OnLoadInteractedEncounters(SaveLoadRequest<InteractedEncounterInfoListModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Loading interacted encounters failed with status " + result.Status + " and error:\n" + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Loaded interacted encounters from " + result.Model.Path, LogTypes.Initialization);
			modelMediator.Save(
				UpdateInteractedEncounters(encounters, result.TypedModel),
				saveResult => OnSavedInteractedEncounters(saveResult, done)
			);
		}

		void OnSavedInteractedEncounters(SaveLoadRequest<InteractedEncounterInfoListModel> result, Action<RequestStatus> done)
		{
			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Saving interacted encounters failed with status " + result.Status + " and error:\n" + result.Error);
				done(result.Status);
				return;
			}

			logger.Log("Saved interacted encounters to " + result.Model.Path, LogTypes.Initialization);
			interactedEncounters = result.TypedModel;
			done(RequestStatus.Success);
		}
		#endregion

		InteractedEncounterInfoListModel UpdateInteractedEncounters(List<EncounterInfoModel> allEncounters, InteractedEncounterInfoListModel target)
		{
			var allEncounterIds = allEncounters.Select(e => e.EncounterId.Value);
			var existingTargets = target.Encounters.Value.Where(e => allEncounterIds.Contains(e.EncounterId));
			var newTargets = new List<InteractedEncounterInfoModel>();

			foreach (var encounter in allEncounters)
			{
				var entry = existingTargets.FirstOrDefault(t => t.EncounterId.Value == encounter.EncounterId.Value) ?? new InteractedEncounterInfoModel();
				entry.EncounterId.Value = encounter.EncounterId;
				newTargets.Add(entry);
			}

			target.Encounters.Value = newTargets.ToArray();

			return target;
		}
	}
}