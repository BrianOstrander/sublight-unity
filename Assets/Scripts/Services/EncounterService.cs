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
		public InteractedEncounterInfoModel GetEncounterInteraction(string encounter)
		{
			return interactedEncounters.GetEncounter(encounter);
		}

		/// <summary>
		/// Gets cached encounters.
		/// </summary>
		public EncounterInfoModel GetEncounter(string encounter)
		{
			return encounters.FirstOrDefault(e => e.EncounterId.Value == encounter);
		}

		/// <summary>
		/// Gets the next best encounter.
		/// </summary>
		/// <remarks>
		/// Will return a "success" result even if none are found.
		/// </remarks>
		public void GetNextEncounter(
			Action<RequestResult, EncounterInfoModel> done,
			EncounterTriggers trigger,
			GameModel model
		)
		{
			if (done == null) throw new ArgumentNullException("done");
			if (trigger == EncounterTriggers.Unknown) throw new ArgumentOutOfRangeException("trigger", "Trigger \"" + trigger + "\" not supported");
			if (model == null) throw new ArgumentNullException("model");

			var remaining = encounters.Where(e => !e.Ignore.Value && e.Trigger.Value == trigger).OrderByDescending(e => e.OrderWeight.Value);

			if (remaining.None())
			{
				done(RequestResult.Success(), null);
				return;
			}

			OnGetNextEncounterFilter(
				done,
				model,
				remaining.First().OrderWeight.Value,
				remaining.ToList(),
				new List<EncounterInfoModel>()
			);
		}
		#endregion

		#region Assign Best Encounter Events
		void OnGetNextEncounterFilter(
			Action<RequestResult, EncounterInfoModel> done,
			GameModel model,
			int orderWeight,
			List<EncounterInfoModel> remaining,
			List<EncounterInfoModel> filtered,
			EncounterInfoModel lastFiltered = null
		)
		{
			if (lastFiltered != null) filtered.Add(lastFiltered);

			if (remaining.None())
			{
				done(RequestResult.Success(), OnGetNextEncounterSelect(filtered));
				return;
			}

			var next = remaining.First();
			remaining.RemoveAt(0);

			if (orderWeight != next.OrderWeight && filtered.Any())
			{
				done(RequestResult.Success(), OnGetNextEncounterSelect(filtered));
				return;
			}

			if (!Mathf.Approximately(next.RandomAppearance.Value, 1f) && next.RandomAppearance.Value < DemonUtility.NextFloat)
			{
				// Skip this, it didn't randomly appear.
				OnGetNextEncounterFilter(
					done,
					model,
					next.OrderWeight,
					remaining,
					filtered
				);
				return;
			}

			valueFilter.Filter(
				filterResult =>
				{
					OnGetNextEncounterFilter(
						done,
						model,
						next.OrderWeight,
						remaining,
						filtered,
						filterResult ? next : null
					);
				},
				next.Filtering,
				model,
				next
			);
		}

		EncounterInfoModel OnGetNextEncounterSelect(
			List<EncounterInfoModel> filtered
		)
		{
			if (filtered.None()) return null;

			var ordered = filtered.OrderBy(f => f.RandomWeightMultiplier.Value);
			var keyed = new List<KeyValuePair<float, EncounterInfoModel>>();
			var offset = 0f;
			foreach (var encounter in ordered)
			{
				offset += encounter.RandomWeightMultiplier.Value;
				keyed.Add(new KeyValuePair<float, EncounterInfoModel>(offset, encounter));
			}
			var selectedOffset = DemonUtility.GetNextFloat(max: offset);

			var lastOffset = 0f;
			foreach (var entry in keyed)
			{
				if ((Mathf.Approximately(entry.Key, lastOffset) || lastOffset < selectedOffset) && selectedOffset < entry.Key)
				{
					return entry.Value;
				}
			}
			return keyed.Last().Value;
		}
		#endregion

		#region Save Events
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