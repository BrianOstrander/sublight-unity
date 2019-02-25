using System;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class TransitPresenter : Presenter<ITransitView>
	{
		GameModel model;
		TransitLanguageBlock language;

		TransitState lastState;
		bool isCompleting;
		Action popSaveBlocker;

		public TransitPresenter(
			GameModel model,
			TransitLanguageBlock language
		)
		{
			this.model = model;
			this.language = language;

			App.Heartbeat.Update += OnUpdate;

			model.Context.TransitStateRequest.Changed += OnTransitStateRequest;
			model.Context.TransitState.Changed += OnTransitState;
		}

		protected override void OnUnBind()
		{
			App.Heartbeat.Update -= OnUpdate;

			model.Context.TransitStateRequest.Changed -= OnTransitStateRequest;
			model.Context.TransitState.Changed -= OnTransitState;
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
			if (isCompleting) return;
			if (model.Context.TransitState.Value.State == TransitState.States.Active) OnProcessState(model.Context.TransitState.Value, delta);
		}

		void OnTransitStateRequest(TransitStateRequest transitStateRequest)
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

			isCompleting = false;

			var transitState = new TransitState
			{
				Instant = transitStateRequest.Instant,
				State = TransitState.States.Request,
				Step = TransitState.Steps.Prepare,
				BeginSystem = transitStateRequest.BeginSystem,
				EndSystem = transitStateRequest.EndSystem
			};

			transitState.VelocityProgress = 0f;
			transitState.VelocityLightYearsCurrent = 0f;
			transitState.VelocityLightYearsMaximum = model.Ship.Velocity.Value.Current.RelativisticLightYears;

			transitState.DistanceProgress = 0f;
			transitState.DistanceTotal = UniversePosition.Distance(transitState.BeginSystem.Position, transitState.EndSystem.Position);
			transitState.DistanceElapsed = 0f;
			transitState.DistanceRemaining = transitState.DistanceTotal;
			transitState.CurrentPosition = transitState.BeginSystem.Position;

			transitState.RelativeTimeProgress = 0f;
			transitState.RelativeTimeElapsed = RelativeDayTime.Zero;

			transitState.BeginRelativeDayTime = model.RelativeDayTime;

			var relativeDurationDelta = RelativityUtility.TransitTime(transitState.VelocityLightYearsMaximum, UniversePosition.ToLightYearDistance(transitState.DistanceTotal));

			transitState.EndRelativeDayTime = model.RelativeDayTime + relativeDurationDelta;

			transitState.LengthScalar = 0f;

			transitState.RelativeTimeTotal = relativeDurationDelta;

			transitState.RelativeTimeRemaining = relativeDurationDelta;
			transitState.RelativeTimeCurrent = transitState.BeginRelativeDayTime;
			transitState.RelativeTimeScalar = 0f;

			transitState.PrepareStep = new TransitState.StepDetails
			{
				Step = TransitState.Steps.Prepare,
				Initializing = false,
				Duration = View.PrepareDuration,
				Elapsed = 0f,
				Progress = 0f
			};

			transitState.TransitStep = new TransitState.StepDetails
			{
				Step = TransitState.Steps.Transit,
				Initializing = false,
				Duration = View.GetTransitDuration(transitState.LengthScalar),
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

			// We have to send a request event to all listeners... kinda redundent but whatever...
			model.Context.TransitState.Value = transitState;

			transitState = transitState.Duplicate;

			if (transitState.Instant)
			{
				transitState.Step = TransitState.Steps.Finalize;

				transitState.PrepareStep = CompleteStep(transitState.PrepareStep);
				transitState.TransitStep = CompleteStep(transitState.TransitStep);
				transitState.FinalizeStep = CompleteStep(transitState.FinalizeStep);
			}
			else
			{
				transitState.PrepareStep.Initializing = true;

				transitState.State = TransitState.States.Active;
				transitState.Step = TransitState.Steps.Prepare;
			}

			model.Context.TransitState.Value = transitState;
			if (transitState.Instant) OnProcessState(transitState, 1f);
			else OnProcessVisuals(transitState);
		}

		void OnTransitState(TransitState transitState)
		{
			switch (transitState.State)
			{
				case TransitState.States.Request:
					popSaveBlocker = model.Context.SaveBlockers.Push(language.SaveDisabledDuringTransit);
					break;
				case TransitState.States.Complete:
					popSaveBlocker();
					break;
			}

			lastState = transitState;
		}

		void OnProcessState(TransitState transitState, float delta)
		{
			transitState = transitState.Duplicate;

			var currentStep = transitState.CurrentStep;
			currentStep.Elapsed = Mathf.Min(currentStep.Duration, currentStep.Elapsed + delta);
			currentStep.Progress = currentStep.Elapsed / currentStep.Duration;

			transitState.CurrentStep = currentStep;

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

			switch (currentStep.Step)
			{
				case TransitState.Steps.Transit:
					transitState.DistanceProgress = View.TransitDistanceCurve.Evaluate(currentStep.Progress);
					transitState.DistanceElapsed = transitState.DistanceProgress * transitState.DistanceTotal;
					transitState.DistanceRemaining = transitState.DistanceTotal - transitState.DistanceElapsed;
					transitState.CurrentPosition = UniversePosition.Lerp(transitState.DistanceProgress, transitState.BeginSystem.Position.Value, transitState.EndSystem.Position.Value);

					View.GetTimeProgress(
						currentStep.Progress,
						transitState.LengthScalar,
						transitState.RelativeTimeTotal,
						out transitState.RelativeTimeProgress,
						out transitState.RelativeTimeElapsed
					);

					transitState.RelativeTimeRemaining = transitState.RelativeTimeTotal - transitState.RelativeTimeElapsed;
					transitState.RelativeTimeCurrent = transitState.BeginRelativeDayTime + transitState.RelativeTimeElapsed;
					transitState.RelativeTimeScalar = View.TransitTimeScalarCurve.Evaluate(currentStep.Progress);

					transitState.VelocityProgress = View.TransitVelocityCurve.Evaluate(currentStep.Progress);
					transitState.VelocityLightYearsCurrent = transitState.VelocityProgress * transitState.VelocityLightYearsMaximum;
					break;
				case TransitState.Steps.Finalize:
					transitState.DistanceProgress = 1f;
					transitState.DistanceElapsed = transitState.DistanceTotal;
					transitState.DistanceRemaining = 0f;
					transitState.CurrentPosition = transitState.EndSystem.Position.Value;

					transitState.RelativeTimeProgress = 1f;
					transitState.RelativeTimeElapsed = transitState.RelativeTimeTotal;
					transitState.RelativeTimeRemaining = RelativeDayTime.Zero;
					transitState.RelativeTimeCurrent = transitState.EndRelativeDayTime;
					transitState.RelativeTimeScalar = 0f;

					transitState.VelocityProgress = 1f;
					transitState.VelocityLightYearsCurrent = 0f;
					break;
			}

			if (transitState.Instant)
			{
				model.Context.TransitState.Value = transitState;
				return;
			}

			OnProcessVisuals(transitState);

			if (transitState.State != TransitState.States.Complete) model.Context.TransitState.Value = transitState;
		}

		void OnProcessVisuals(TransitState transitState)
		{
			switch (transitState.State)
			{
				case TransitState.States.Active:
					var details = transitState.CurrentStep;
					switch(transitState.Step)
					{
						case TransitState.Steps.Prepare:
							if (details.Initializing) OnProcessVisualsPrepareInitialize(transitState, details);
							OnProcessVisualsPrepare(transitState, details);
							break;
						case TransitState.Steps.Transit:
							if (details.Initializing) OnProcessVisualsTransitInitialize(transitState, details);
							OnProcessVisualsTransit(transitState, details);
							break;
						case TransitState.Steps.Finalize:
							if (details.Initializing) OnProcessVisualsFinalizeInitialize(transitState, details);
							OnProcessVisualsFinalize(transitState, details);
							break;
					}
					break;
				default:
					isCompleting = true;
					OnProcessVisualsComplete(transitState);
					break;
			}
		}

		void OnProcessVisualsPrepareInitialize(TransitState transitState, TransitState.StepDetails details)
		{
			View.Reset();

			ShowView();

			App.Callbacks.SetFocusRequest(SetFocusRequest.Request(GameState.Focuses.GetPriorityFocus(ToolbarSelections.System)));
		}

		void OnProcessVisualsPrepare(TransitState transitState, TransitState.StepDetails details)
		{

		}

		void OnProcessVisualsTransitInitialize(TransitState transitState, TransitState.StepDetails details)
		{

		}

		void OnProcessVisualsTransit(TransitState transitState, TransitState.StepDetails details)
		{

		}

		void OnProcessVisualsFinalizeInitialize(TransitState transitState, TransitState.StepDetails details)
		{

		}

		void OnProcessVisualsFinalize(TransitState transitState, TransitState.StepDetails details)
		{

		}

		void OnProcessVisualsComplete(TransitState transitState)
		{
			CloseView(true);
			App.Callbacks.SetFocusRequest(
				SetFocusRequest.Request(
					GameState.Focuses.GetToolbarSelectionFocus(ToolbarSelections.System),
					() => model.Context.TransitState.Value = transitState
				)
			);
		}
		#endregion
	}
}