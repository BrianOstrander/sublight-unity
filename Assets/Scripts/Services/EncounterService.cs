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
		CallbackService callbacks;
		ValueFilterService valueFilter;

		List<EncounterInfoModel> encounters = new List<EncounterInfoModel>();
		InteractedEncounterInfoListModel interactedEncounters;
		bool currentlySaving;

		public EncounterService(
			IModelMediator modelMediator,
			CallbackService callbacks,
			ValueFilterService valueFilter
		)
		{
			this.modelMediator = modelMediator ?? throw new ArgumentNullException(nameof(modelMediator));
			this.callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
			this.valueFilter = valueFilter ?? throw new ArgumentNullException(nameof(valueFilter));
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

			if (DevPrefs.LoggingInitialization) Debug.Log("Loaded " + encounters.Count + " encounters");

			if (result.Length == 0)
			{
				if (DevPrefs.LoggingInitialization) Debug.Log("No existing interacted encounters, generating defaults");
				modelMediator.Save(
					UpdateInteractedEncounters(encounters, modelMediator.Create<InteractedEncounterInfoListModel>(App.M.CreateUniqueId())),
					saveResult => OnSavedInteractedEncounters(saveResult, done)
				);
			}
			else
			{
				var toLoad = result.Models.Where(p => p.SupportedVersion.Value).OrderBy(p => p.Version.Value).LastOrDefault();
				if (toLoad == null)
				{
					if (DevPrefs.LoggingInitialization) Debug.Log("No supported interacted encounters, generating defaults");
					modelMediator.Save(
						UpdateInteractedEncounters(encounters, modelMediator.Create<InteractedEncounterInfoListModel>(App.M.CreateUniqueId())),
						saveResult => OnSavedInteractedEncounters(saveResult, done)
					);
				}
				else
				{
					if (DevPrefs.LoggingInitialization) Debug.Log("Loading existing interacted encounters");
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

			if (DevPrefs.LoggingInitialization) Debug.Log("Loaded interacted encounters from " + result.Model.Path);
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

			if (DevPrefs.LoggingInitialization) Debug.Log("Saved interacted encounters to " + result.Model.Path);

			currentlySaving = false;
			interactedEncounters = result.TypedModel;
			callbacks.SaveRequest += OnSaveRequest;

			done(RequestStatus.Success);
		}

		InteractedEncounterInfoListModel UpdateInteractedEncounters(List<EncounterInfoModel> allEncounters, InteractedEncounterInfoListModel target)
		{
			var allEncounterIds = allEncounters.Select(e => e.Id.Value);
			var existingTargets = target.Encounters.Value.Where(e => allEncounterIds.Contains(e.EncounterId));
			var newTargets = new List<InteractedEncounterInfoModel>();

			foreach (var encounter in allEncounters)
			{
				var entry = existingTargets.FirstOrDefault(t => t.EncounterId.Value == encounter.Id.Value);
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
			return encounters.FirstOrDefault(e => e.Id.Value == encounter);
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
				OnGetNextEncounterSelect(
					done,
					model,
					remaining,
					filtered
				);
				return;
			}

			var next = remaining.First();

			if (orderWeight != next.OrderWeight && filtered.Any())
			{
				OnGetNextEncounterSelect(
					done,
					model,
					remaining,
					filtered
				);
				return;
			}

			remaining.RemoveAt(0);

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

		void OnGetNextEncounterSelect(
			Action<RequestResult, EncounterInfoModel> done,
			GameModel model,
			List<EncounterInfoModel> remaining,
			List<EncounterInfoModel> filtered
		)
		{
			if (filtered.None())
			{
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

				return;
			}

			done(
				RequestResult.Success(),
				filtered.RandomWeighted(
					e => e.RandomWeightMultiplier.Value
				)
			);

			/*
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
					done(RequestResult.Success(), entry.Value);
					return;
				}
			}
			done(RequestResult.Success(), keyed.Last().Value);
			*/
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