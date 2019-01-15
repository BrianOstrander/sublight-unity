using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class EncounterService
	{
		IModelMediator modelMediator;
		ILogService logger;
		CallbackService callbacks;
		ValueFilterService valueFilter;

		List<EncounterInfoModel> encounters = new List<EncounterInfoModel>();
		InteractedEncounterInfoListModel interactedEncounters;
		bool currentlySaving;

		public EncounterService(IModelMediator modelMediator, ILogService logger, CallbackService callbacks, ValueFilterService valueFilter)
		{
			if (modelMediator == null) throw new ArgumentNullException("modelMediator");
			if (logger == null) throw new ArgumentNullException("logger");
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (valueFilter == null) throw new ArgumentNullException("valueFilter");

			this.modelMediator = modelMediator;
			this.logger = logger;
			this.callbacks = callbacks;
			this.valueFilter = valueFilter;
		}

		#region Initialization
		public void Initialize(Action<RequestStatus> done)
		{
			currentlySaving = true;
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

			currentlySaving = false;
			interactedEncounters = result.TypedModel;
			callbacks.SaveRequest += OnSaveRequest;

			done(RequestStatus.Success);
		}

		InteractedEncounterInfoListModel UpdateInteractedEncounters(List<EncounterInfoModel> allEncounters, InteractedEncounterInfoListModel target)
		{
			var allEncounterIds = allEncounters.Select(e => e.EncounterId.Value);
			var existingTargets = target.Encounters.Value.Where(e => allEncounterIds.Contains(e.EncounterId));
			var newTargets = new List<InteractedEncounterInfoModel>();

			foreach (var encounter in allEncounters)
			{
				var entry = existingTargets.FirstOrDefault(t => t.EncounterId.Value == encounter.EncounterId.Value);
				if (entry == null) continue;
				newTargets.Add(entry);
			}

			target.Encounters.Value = newTargets.ToArray();

			return target;
		}
		#endregion

		#region Utility
		public void AssignBestEncounter(Action<AssignBestEncounter> done, GameModel model, SystemModel system)
		{
			// Required checks
			var remaining = encounters.Where(
				e =>
				{
					if (e.Ignore.Value) return false;
					switch (model.EncounterState.GetEncounterStatus(e.EncounterId).State)
					{
						case EncounterStatus.States.Completed:
						case EncounterStatus.States.Seen:
							return false;
					}
					return true;
				}
			);

			if (remaining.None())
			{
				OnAssignBestEncounterResult(done, null, system);
				return;
			}

			OnFilterEncounters(
				done,
				null,
				null,
				remaining.ToList(),
				new List<EncounterInfoModel>(),
				model,
				system
			);
		}

		/// <summary>
		/// Gets cached encounters.
		/// </summary>
		public EncounterInfoModel GetEncounter(string encounter)
		{
			return encounters.FirstOrDefault(e => e.EncounterId.Value == encounter);
		}

		public InteractedEncounterInfoModel GetEncounterInteraction(string encounter)
		{
			return interactedEncounters.GetEncounter(encounter);
		}
		#endregion

		#region Events
		void OnFilterEncounters(
			Action<AssignBestEncounter> done,
			bool? lastFilteredPassed,
			EncounterInfoModel lastFiltered,
			List<EncounterInfoModel> toFilter,
			List<EncounterInfoModel> filtered,
			GameModel model,
			SystemModel system
		)
		{
			if (lastFilteredPassed.HasValue)
			{
				if (lastFilteredPassed.Value) filtered.Add(lastFiltered);
			}

			if (toFilter.Any())
			{
				var nextToFilter = toFilter.First();
				toFilter.RemoveAt(0);
				valueFilter.Filter(result =>
				{
					OnFilterEncounters(done, result, nextToFilter, toFilter, filtered, model, system);
				}, nextToFilter.Filtering, model);
				return;
			}

			if (filtered.None())
			{
				OnAssignBestEncounterResult(done, null, system);
				return;
			}

			var ordered = filtered.OrderByDescending(r => r.OrderWeight.Value);
			var topWeight = ordered.First().OrderWeight.Value;

			var maxRandomWeight = 0f;
			EncounterInfoModel chosen = null;

			foreach (var current in ordered.Where(r => Mathf.Approximately(r.OrderWeight.Value, topWeight)))
			{
				var currentWeight = current.RandomWeightMultiplier.Value * DemonUtility.NextFloat;
				if (chosen == null || maxRandomWeight < currentWeight)
				{
					maxRandomWeight = currentWeight;
					chosen = current;
				}
			}

			model.EncounterState.SetEncounterStatus(EncounterStatus.Seen(chosen.EncounterId));

			var interaction = GetEncounterInteraction(chosen.EncounterId);
			interaction.TimesSeen.Value++;
			interaction.LastSeen.Value = DateTime.Now;

			OnFilterEncountersDone(done, chosen, system);
		}

		void OnFilterEncountersDone(Action<AssignBestEncounter> done, EncounterInfoModel encounter, SystemModel system)
		{
			OnAssignBestEncounterResult(done, encounter, system);
		}

		void OnAssignBestEncounterResult(Action<AssignBestEncounter> done, EncounterInfoModel encounter, SystemModel system)
		{
			if (encounter == null)
			{
				done(SubLight.AssignBestEncounter.NoEncounterResult());
				return;
			}
			done(SubLight.AssignBestEncounter.EncounterResult(encounter));
		}

		void OnSaveRequest(SaveRequest request)
		{
			if (request.State == SaveRequest.States.Complete) OnTrySave();
		}

		void OnTrySave()
		{
			if (currentlySaving) return;
			modelMediator.Save(interactedEncounters, OnTrySaved);
		}

		void OnTrySaved(SaveLoadRequest<InteractedEncounterInfoListModel> result)
		{
			currentlySaving = false;

			if (result.Status != RequestStatus.Success)
			{
				Debug.LogError("Trying to save interacted encounter info list failed with status " + result.Status + "\nError: " + result.Error);
			}
		}
		#endregion
	}
}