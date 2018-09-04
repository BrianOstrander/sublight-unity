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

			defaultTexture = new RenderTexture(1, 1, 16);

			callbacks.SetFocusRequest += OnSetFocusRequest;
			callbacks.TransitionFocusRequest += OnTransitionFocusRequest;
			callbacks.StateChange += OnStateChange;
			heartbeat.Update += OnUpdate;
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
				currents = lastActive.Targets;
				supported = defaults.Select(d => d.Layer).Distinct().ToArray();
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
			Debug.LogWarning("Todo: handle non default focus! Trigger a gather focus request here...");
		}

		void OnTransitionFocusRequest(TransitionFocusRequest request)
		{
			lastTransition = request;

			switch (request.State)
			{
				case TransitionFocusRequest.States.Request:
					state = States.Active;
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
			if (state == States.Complete) return;
		}

		void OnTransitionActive(TransitionFocusRequest transition)
		{
			if (Mathf.Approximately(1f, transition.Progress))
			{
				// We're done here...
				callbacks.TransitionFocusRequest(transition.Duplicate(TransitionFocusRequest.States.Complete));
				return;
			}

			Debug.LogWarning("Todo: handle non instant transitions here!");
		}

		void OnTransitionComplete()
		{
			state = States.Complete;
			lastActive.Done();
		}
		#endregion
		SetFocusTransition[] BuildTransitions(SetFocusBlock[] ends, bool includeIdentical)
		{
			var results = new List<SetFocusTransition>();

			foreach (var end in ends)
			{
				if (!supported.Contains(end.Layer))
				{
					Debug.LogError("Layer " + end.Layer + " not currently supported. Make sure to include it when setting defaults.");
					continue;
				}
				var current = currents.FirstOrDefault(c => c.Layer == end.Layer);

				if (!current.HasDelta(end) && !includeIdentical) continue;

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
	}
}