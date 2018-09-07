using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class FocusService
	{
		enum States
		{
			Unknown = 0,
			Initializing = 10,
			Active = 20,
			Complete = 30
		}

		Heartbeat heartbeat;
		CallbackService callbacks;
		Func<PreferencesModel> currentPreferences;
		RenderTexture defaultTexture;

		Dictionary<SetFocusLayers, int> camerasPerLayer = new Dictionary<SetFocusLayers, int>();
		States state = States.Complete;
		/// <summary>
		/// The supported layers. If we get a request with an unrecognized one,
		/// warn the console.
		/// </summary>
		SetFocusLayers[] supported;
		SetFocusBlock[] defaults;
		SetFocusBlock[] currents;

		SetFocusRequest lastActive;
		TransitionFocusRequest lastTransition;

		public FocusService(
			Heartbeat heartbeat,
			CallbackService callbacks,
			Func<PreferencesModel> currentPreferences
		)
		{
			if (heartbeat == null) throw new ArgumentNullException("heartbeat");
			if (callbacks == null) throw new ArgumentNullException("callbacks");
			if (currentPreferences == null) throw new ArgumentNullException("currentPreferences");

			this.heartbeat = heartbeat;
			this.callbacks = callbacks;
			this.currentPreferences = currentPreferences;

			defaultTexture = null;

			callbacks.SetFocusRequest += OnSetFocusRequest;
			callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
			callbacks.StateChange += OnStateChange;
			heartbeat.Update += OnUpdate;
		}

		public void RegisterLayer(SetFocusLayers layer)
		{
			if (camerasPerLayer.ContainsKey(layer)) camerasPerLayer[layer]++;
			else camerasPerLayer[layer] = 1;
		}

		public void UnRegisterLayer(SetFocusLayers layer)
		{
			if (camerasPerLayer.ContainsKey(layer)) camerasPerLayer[layer]--;
			else camerasPerLayer[layer] = 0;
		}

		public void SetOnTransitionFocusRequestLast()
		{
			// This is kind of hacky...
			callbacks.TransitionFocusRequest -= OnTransitionFocusRequest;
			callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
		}

		#region Events
		void OnSetFocusRequest(SetFocusRequest request)
		{
			if (state != States.Complete)
			{
				Debug.LogError("Unable to set focus while handling another one.");
				return;
			}

			state = States.Initializing;
			lastActive = request;

			if (lastActive.IsDefault)
			{
				defaults = lastActive.Targets;
				supported = defaults.Select(d => d.Layer).Distinct().ToArray();
				OnCheckRegistrations();
			}

			var transitions = BuildTransitions(lastActive.Targets, lastActive.IsDefault);

			if (lastActive.IsDefault)
			{
				callbacks.TransitionFocusRequest(
					TransitionFocusRequest.RequestInstant(
						BuildDefaultGatherResults(),
						transitions
					)
				);
				return;
			}

			var gatherRequest = BuildGatherRequest(request, transitions);

			if (gatherRequest.NoRequests)
			{
				gatherRequest.Done(GatherFocusResult.Empty);
				return;
			}
			callbacks.GatherFocusRequest(gatherRequest);
		}

		void OnCheckRegistrations()
		{
			var multiRegistrations = new Dictionary<SetFocusLayers, int>();
			var noRegistrations = new List<SetFocusLayers>();

			foreach(var layer in supported)
			{
				int camerasForLayer;
				if (camerasPerLayer.TryGetValue(layer, out camerasForLayer))
				{
					if (camerasForLayer == 0) noRegistrations.Add(layer);
					else if (1 < camerasForLayer) multiRegistrations.Add(layer, camerasForLayer);
				}
				else noRegistrations.Add(layer);
			}

			if (0 < multiRegistrations.Count)
			{
				var result = string.Empty;
				foreach (var entry in multiRegistrations) result += "\n\t"+entry.Key + ": " + entry.Value;
				Debug.LogError("Multiple camera layer registrations for the following:" + result);
			}
			if (0 < noRegistrations.Count)
			{
				var result = string.Empty;
				foreach (var layer in noRegistrations) result += "\n\t" + layer;
				Debug.LogError("No camera layer registrations for the following:" + result);
			}
		}

		void OnTransitionFocusRequest(TransitionFocusRequest request)
		{
			Debug.Log("Transition: " + request.State + ", Progress: " + request.Progress);
			lastTransition = request;

			switch (request.State)
			{
				case TransitionFocusRequest.States.Request:
					state = States.Active;
					SetOnTransitionFocusRequestLast();
					callbacks.TransitionFocusRequest(request.Duplicate(TransitionFocusRequest.States.Active, request.Instant ? 1f : 0f));
					break;
				case TransitionFocusRequest.States.Active:
					OnTransitionActive(request);
					break;
				case TransitionFocusRequest.States.Complete:
					OnTransitionComplete();
					break;
				default:
					Debug.LogError("Unrecognized State: " + request.State);
					break;
			}
		}

		void OnStateChange(StateChange change)
		{
			if (state == States.Complete) return;
			Debug.LogWarning("Todo: handle interrupt focusing...");
		}

		void OnUpdate(float delta)
		{
			if (state == States.Complete || lastTransition.State != TransitionFocusRequest.States.Active) return;
			Debug.Log("caling from update");
			var currentProgress = Mathf.Clamp01((float)((DateTime.Now - lastTransition.StartTime).TotalSeconds / lastTransition.Duration.TotalSeconds));
			callbacks.TransitionFocusRequest(lastTransition.Duplicate(progress: currentProgress));
		}

		void OnTransitionActive(TransitionFocusRequest transition)
		{
			if (Mathf.Approximately(1f, transition.Progress))
			{
				// We're done here...
				callbacks.TransitionFocusRequest(transition.Duplicate(TransitionFocusRequest.States.Complete));
			}
		}

		void OnTransitionComplete()
		{
			state = States.Complete;
			currents = AddDefaults(lastActive.Targets);
			lastActive.Done();
			Debug.Log("we should be done...");
		}

		void OnGatherFocusResult(
			DeliverFocusBlock next,
			List<DeliverFocusBlock> results,
			List<SetFocusLayers> remainingLayers,
			Action<GatherFocusResult> done
		)
		{
			if (remainingLayers.None()) return;

			results.Add(next);
			remainingLayers.RemoveAll(l => l == next.Layer);

			if (remainingLayers.Any()) return;

			done(new GatherFocusResult(results.ToArray()));
		}

		void OnGatherFocusResultDone(SetFocusRequest request, SetFocusTransition[] transitions, GatherFocusResult result)
		{
			TransitionFocusRequest transitionRequest;
			if (request.Instant) transitionRequest = TransitionFocusRequest.RequestInstant(result, transitions);
			else
			{
				var startTime = DateTime.Now;
				var endTime = startTime.Add(TimeSpan.FromSeconds(request.Duration));
				transitionRequest = TransitionFocusRequest.Request(result, transitions, startTime, endTime);
			}
			callbacks.TransitionFocusRequest(transitionRequest);
		}
		#endregion
		SetFocusTransition[] BuildTransitions(SetFocusBlock[] ends, bool includeIdentical)
		{
			var results = new List<SetFocusTransition>();

			foreach (var end in AddDefaults(ends))
			{
				if (!supported.Contains(end.Layer))
				{
					Debug.LogError("Layer " + end.Layer + " not currently supported. Make sure to include it when setting defaults.");
					continue;
				}
				var current = (currents ?? defaults).FirstOrDefault(c => c.Layer == end.Layer);

				if (!(includeIdentical || end.Enabled || current.HasDelta(end))) continue;

				results.Add(new SetFocusTransition(end.Layer, current, end));
			}

			return results.ToArray();
		}

		GatherFocusResult BuildDefaultGatherResults()
		{
			var blocks = new List<DeliverFocusBlock>();

			foreach (var layer in supported) blocks.Add(new DeliverFocusBlock(layer, defaultTexture));

			return new GatherFocusResult(blocks.ToArray());
		}

		GatherFocusRequest BuildGatherRequest(SetFocusRequest request, SetFocusTransition[] transitions)
		{
			var remainingLayers =  request.Targets.Select(t => t.Layer).ToList();
			var gatherRequests = new List<DeliverFocusBlock>();
			var gatherResults = new List<DeliverFocusBlock>();
			Action<GatherFocusResult> onResultDone = result => OnGatherFocusResultDone(request, transitions, result);
			Action<DeliverFocusBlock> onResult = result => OnGatherFocusResult(result, gatherResults, remainingLayers, onResultDone);
			foreach (var transition in transitions) gatherRequests.Add(new DeliverFocusBlock(transition.Layer, onResult));

			return GatherFocusRequest.Request(onResultDone, gatherRequests.ToArray());
		}

		SetFocusBlock[] AddDefaults(SetFocusBlock[] blocks)
		{
			var representedLayers = blocks.Select(e => e.Layer);
			return blocks.Concat(defaults.Where(c => !representedLayers.Contains(c.Layer))).ToArray();
		}
	}
}