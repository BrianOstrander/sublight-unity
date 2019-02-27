using System;
using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight
{
	public struct TransitionFocusRequest
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		public static TransitionFocusRequest Request(
			GatherFocusResult gatherResult,
			SetFocusTransition[] transitions,
			float duration
		)
		{
			var instant = Mathf.Approximately(0f, duration);
			if (instant) Debug.LogWarning("Requesting a focus with a duration of zero, running instantly");

			return new TransitionFocusRequest(
				States.Request,
				gatherResult,
				transitions,
				0f,
				duration,
				0f,
				instant,
				false
			);
		}

		public static TransitionFocusRequest RequestInstant(
			GatherFocusResult gatherResult,
			SetFocusTransition[] transitions
		)
		{
			return new TransitionFocusRequest(
				States.Request,
				gatherResult,
				transitions,
				0f,
				0f,
				0f,
				true,
				false
			);
		}

		public readonly States State;
		public readonly GatherFocusResult GatherResult;
		public readonly SetFocusTransition[] Transitions;
		public readonly float Elapsed;
		public readonly float Duration;
		public readonly float Progress;
		public readonly bool Instant;
		/// <summary>
		/// The first active called, not garaunteed to have a progress of 0.
		/// </summary>
		public readonly bool FirstActive;
		/// <summary>
		/// Every transition, instant or not, is garaunteed to end with at least
		/// one last call where progress equals 1. There is no garauntee one
		/// will be called with 0.
		/// </summary>
		public readonly bool LastActive;

		public TransitionFocusRequest(
			States state,
			GatherFocusResult gatherResult,
			SetFocusTransition[] transitions,
			float elapsed,
			float duration,
			float progress,
			bool instant,
			bool firstActive
		)
		{
			State = state;
			GatherResult = gatherResult;
			Transitions = transitions;
			Elapsed = elapsed;
			Duration = duration;
			Progress = progress;
			Instant = instant;

			FirstActive = State == States.Active && (instant || firstActive);
			LastActive = State == States.Active && Mathf.Approximately(1f, progress);
		}

		public TransitionFocusRequest Duplicate(
			bool firstActive,
			States state = States.Unknown,
			float? elapsed = null,
			float? progress = null
		)
		{
			return new TransitionFocusRequest(
				state == States.Unknown ? State : state,
				GatherResult,
				Transitions,
				elapsed.HasValue ? elapsed.Value : Elapsed,
				Duration,
				progress.HasValue ? progress.Value : Progress,
				Instant,
				firstActive
			);
		}

		public bool GetTransition(SetFocusLayers layer, out SetFocusTransition result)
		{
			if (layer == SetFocusLayers.Unknown)
			{
				Debug.Log("Layer " + layer + " is not supported.");
				result = default(SetFocusTransition);
				return false;
			}
			result = Transitions.FirstOrDefault(t => t.Layer == layer);
			return !result.NoTransition;
		}
	}
}