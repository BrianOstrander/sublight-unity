using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	/// <summary>
	/// Transit state. Some of these values may not be initialized when the game
	/// starts... not sure how I feel about that, but just keep that in mind.
	/// </summary>
	public class TransitState
	{
		public enum States
		{
			Unknown = 0,
			Request = 10,
			Active = 20,
			Complete = 30
		}

		public enum Steps
		{
			Unknown = 0,
			Prepare = 10,
			Transit = 20,
			Finalize = 30,
		}

		public class StepDetails
		{
			public Steps Step;
			public bool Initializing;
			public float Duration;
			public float Elapsed;
			public float Progress;

			public StepDetails Duplicate
			{
				get
				{
					return new StepDetails
					{
						Step = Step,
						Initializing = Initializing,
						Duration = Duration,
						Elapsed = Elapsed,
						Progress = Progress
					};
				}
			}
		}

		public static TransitState Default(
			SystemModel beginSystem = null,
			SystemModel endSystem = null
		)
		{
			return new TransitState
			{
				State = States.Complete,
				Step = Steps.Finalize,
				BeginSystem = beginSystem,
				EndSystem = endSystem
			};
		}

		#region Unchanged
		public bool Instant;
		public SystemModel BeginSystem;
		public SystemModel EndSystem;
		public RelativeDayTime BeginRelativeDayTime;
		public RelativeDayTime EndRelativeDayTime;

		/// <summary>
		/// How long thould this take on a scale of shortest possible transit to longest? 0 to 1.
		/// </summary>
		public float LengthScalar;

		public float TotalDuration { get { return PrepareStep.Duration + TransitStep.Duration + FinalizeStep.Duration; } }
		#endregion

		#region Dynamic
		public States State;
		public Steps Step;

		public float TotalElapsed { get { return PrepareStep.Elapsed + TransitStep.Elapsed + FinalizeStep.Elapsed; } }
		public float TotalProgress { get { return TotalElapsed / TotalDuration; } }
		public float AnimationProgress { get { return PrepareStep.Progress + TransitStep.Progress + FinalizeStep.Progress; } }

		public StepDetails PrepareStep = new StepDetails { Step = Steps.Prepare };
		public StepDetails TransitStep = new StepDetails { Step = Steps.Transit };
		public StepDetails FinalizeStep = new StepDetails { Step = Steps.Finalize };

		public StepDetails CurrentStep
		{
			get
			{
				switch (Step)
				{
					case Steps.Prepare: return PrepareStep;
					case Steps.Transit : return TransitStep;
					case Steps.Finalize: return FinalizeStep;
					default:
						throw new NotImplementedException("Unrecognized step: " + Step);
				}
			}
			set
			{
				switch (value.Step)
				{
					case Steps.Prepare: PrepareStep = value; break;
					case Steps.Transit: TransitStep = value; break;
					case Steps.Finalize: FinalizeStep = value; break;
					default:
						throw new NotImplementedException("Unrecognized step: " + Step);
				}
			}
		}

		/// <summary>
		/// Gets the requested step, and returns true if it has changed since last time.
		/// </summary>
		/// <returns><c>true</c>, if step has changed, <c>false</c> otherwise.</returns>
		/// <param name="step">Step.</param>
		/// <param name="details">Details.</param>
		public bool GetStep(Steps step, out StepDetails details)
		{
			switch (step)
			{
				case Steps.Prepare: details = PrepareStep; break;
				case Steps.Transit: details = TransitStep; break;
				case Steps.Finalize: details = FinalizeStep; break;
				default: details = default(StepDetails); break;
			}

			if (Step == step) return true;

			if (step == Steps.Prepare && Step == Steps.Transit && TransitStep.Initializing) return true;
			if (step == Steps.Transit && Step == Steps.Finalize && FinalizeStep.Initializing) return true;

			return false;
		}

		public float DistanceProgress;
		public float DistanceTotal;
		public float DistanceElapsed; // In universe units
		public float DistanceRemaining; // In universe units
		public UniversePosition CurrentPosition;

		public float RelativeTimeProgress;
		public RelativeDayTime RelativeTimeTotal;
		public RelativeDayTime RelativeTimeElapsed;
		public RelativeDayTime RelativeTimeRemaining;
		public RelativeDayTime RelativeTimeCurrent;
		public float RelativeTimeScalar;

		public float VelocityProgress; // Should go from 0 to 1 to 0 again.
		public float VelocityLightYearsMaximum; // relative to lightspeed.
		public float VelocityLightYearsCurrent; // relative to lightspeed.
		#endregion

		public TransitState Duplicate
		{
			get
			{
				var result = new TransitState();

				result.Instant = Instant;
				result.BeginSystem = BeginSystem;
				result.EndSystem = EndSystem;
				result.BeginRelativeDayTime = BeginRelativeDayTime;
				result.EndRelativeDayTime = EndRelativeDayTime;

				result.LengthScalar = LengthScalar;

				result.State = State;
				result.Step = Step;

				result.PrepareStep = PrepareStep.Duplicate;
				result.TransitStep = TransitStep.Duplicate;
				result.FinalizeStep = FinalizeStep.Duplicate;

				result.DistanceProgress = DistanceProgress;
				result.DistanceTotal = DistanceTotal;
				result.DistanceElapsed = DistanceElapsed;
				result.DistanceRemaining = DistanceRemaining;
				result.CurrentPosition = CurrentPosition;

				result.RelativeTimeProgress = RelativeTimeProgress;
				result.RelativeTimeTotal = RelativeTimeTotal;
				result.RelativeTimeElapsed = RelativeTimeElapsed;
				result.RelativeTimeRemaining = RelativeTimeRemaining;
				result.RelativeTimeCurrent = RelativeTimeCurrent;
				result.RelativeTimeScalar = RelativeTimeScalar;

				result.VelocityProgress = VelocityProgress;
				result.VelocityLightYearsMaximum = VelocityLightYearsMaximum;
				result.VelocityLightYearsCurrent = VelocityLightYearsCurrent;

				return result;
			}
		}
	}
}