using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class GridTransitLockoutPresenter : FocusPresenter<IGridTransitLockoutView, SystemFocusDetails>
	{
		GameModel model;

		TransitState lastState;

		public GridTransitLockoutPresenter(
			GameModel model
		)
		{
			this.model = model;

			App.Heartbeat.Update += OnUpdate;

			model.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			App.Heartbeat.Update -= OnUpdate;

			model.TransitState.Changed -= OnTransitState;
		}

		protected override void OnUpdateEnabled()
		{

		}

		TransitState.StepDetails CompleteStep(TransitState.StepDetails step)
		{
			step.Initializing = false;
			step.Elapsed = step.Duration;
			step.Progress = 1f;

			return step;
		}

		#region Events
		void OnUpdate(float delta)
		{
			if (model.TransitState.Value != null && model.TransitState.Value.State == TransitState.States.Active) OnProcessState(model.TransitState.Value, delta);
		}

		void OnTransitState(TransitState transitState)
		{
			Debug.Log("TransitState: " + transitState.State + "." + transitState.Step + "." + (transitState.CurrentStep.Initializing ? "Initializing" : "Normal")+" "+transitState.CurrentStep.Progress);
			switch (transitState.State)
			{
				case TransitState.States.Request:
					OnTransitStateRequest(transitState);
					break;
			}

			lastState = transitState;

		}

		void OnTransitStateRequest(TransitState transitState)
		{
			if (lastState != null)
			{
				switch (lastState.State)
				{
					case TransitState.States.Active:
					case TransitState.States.Request:
						Debug.LogError("Requesting a transit state while another is requested or active. Unexpected behaviour may occur.");
						break;
				}
			}

			transitState = transitState.Duplicate;

			transitState.VelocityProgress = 0f;
			transitState.VelocityLightYearsCurrent = 0f;
			transitState.VelocityLightYearsMaximum = model.Ship.Value.Velocity.Value.VelocityLightYearsCurrent;

			transitState.DistanceProgress = 0f;
			transitState.DistanceTotal = UniversePosition.Distance(transitState.BeginSystem.Position, transitState.EndSystem.Position);
			transitState.DistanceElapsed = 0f;
			transitState.DistanceRemaining = transitState.DistanceTotal;
			transitState.CurrentPosition = transitState.BeginSystem.Position;

			transitState.TimeProgress = 0f;
			transitState.TimeElapsed = RelativeDayTime.Zero;

			transitState.BeginRelativeDayTime = model.RelativeDayTime;

			var relativeDurationDelta = RelativityUtility.TransitTime(transitState.VelocityLightYearsMaximum, UniversePosition.ToLightYearDistance(transitState.DistanceTotal));

			transitState.EndRelativeDayTime = new RelativeDayTime
			{
				ShipTime = transitState.BeginRelativeDayTime.ShipTime + relativeDurationDelta.ShipTime,
				GalacticTime = transitState.BeginRelativeDayTime.GalacticTime + relativeDurationDelta.GalacticTime
			};

			transitState.TimeRemaining = relativeDurationDelta;
			transitState.CurrentRelativeDayTime = transitState.BeginRelativeDayTime;

			transitState.State = TransitState.States.Active;
			transitState.Step = TransitState.Steps.Prepare;

			transitState.PrepareStep = new TransitState.StepDetails
			{
				Step = TransitState.Steps.Prepare,
				Initializing = true,
				Duration = View.PrepareDuration,
				Elapsed = 0f,
				Progress = 0f
			};

			transitState.TransitStep = new TransitState.StepDetails
			{
				Step = TransitState.Steps.Transit,
				Initializing = false,
				Duration = View.TransitDuration.x,
				Elapsed = 0f,
				Progress = 0f
			};

			transitState.FinalizeStep = new TransitState.StepDetails
			{
				Step = TransitState.Steps.Finalize,
				Initializing = false,
				Duration = View.FinalizeDuration,
				Elapsed = 0f,
				Progress = 0f
			};

			if (transitState.Instant)
			{
				transitState.Step = TransitState.Steps.Finalize;

				transitState.PrepareStep = CompleteStep(transitState.PrepareStep);
				transitState.TransitStep = CompleteStep(transitState.TransitStep);
				transitState.FinalizeStep = CompleteStep(transitState.FinalizeStep);
			}

			model.TransitState.Value = transitState;
			if (transitState.Instant) OnProcessState(transitState, 1f);
		}

		void OnProcessState(TransitState transitState, float delta)
		{
			transitState = transitState.Duplicate;

			var currentStep = transitState.CurrentStep;
			currentStep.Elapsed = Mathf.Min(currentStep.Duration, currentStep.Elapsed + delta);
			currentStep.Progress = currentStep.Elapsed / currentStep.Duration;

			transitState.CurrentStep = currentStep;
			//Debug.Log("now now val " + transitState.CurrentStep.Elapsed+" and "+currentStep.Elapsed);

			var lastStep = currentStep.Step;

			if (Mathf.Approximately(currentStep.Duration, currentStep.Elapsed))
			{
				// We've completed a step;
				switch (currentStep.Step)
				{
					case TransitState.Steps.Prepare: transitState.Step = TransitState.Steps.Transit; break;
					case TransitState.Steps.Transit: transitState.Step = TransitState.Steps.Finalize; break;
					case TransitState.Steps.Finalize:
						transitState.State = TransitState.States.Complete;
						break;
				}
			}

			if (lastStep != transitState.Step)
			{
				// Step changed
				var nextStep = transitState.CurrentStep;
				nextStep.Initializing = true;
				transitState.CurrentStep = nextStep;
			}
			else
			{
				currentStep = transitState.CurrentStep;
				currentStep.Initializing = false;
				transitState.CurrentStep = currentStep;
			}

			// TODO: Calculate intermediate values here...

			model.TransitState.Value = transitState;
		}
		#endregion
	}
}