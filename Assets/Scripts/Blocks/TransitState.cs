using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct TransitState
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

		public struct StepDetails
		{
			public Steps Step;
			public bool Initializing;
			public float Duration;
			public float Elapsed;
			public float Progress;
		}

		public static TransitState Request(
			bool instant,
			SystemModel beginSystem,
			SystemModel endSystem
		)
		{
			return new TransitState
			{
				Instant = instant,
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
		public float TotalDuration;
		#endregion

		#region Dynamic
		public States State;
		public Steps Step;
		public float TotalElapsed;
		public float TotalProgress;
		public StepDetails PrepareStep;
		public StepDetails TransitStep;
		public StepDetails FinalizeStep;

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
		}

		public float DistanceProgress;
		public float DistanceElapsed; // In universe units
		public float DistanceRemaining; // In universe units
		public UniversePosition CurrentPosition;

		public float TimeProgress;
		public RelativeDayTime TimeElapsed;
		public RelativeDayTime TimeRemaining;
		public RelativeDayTime CurrentRelativeDayTime;
		#endregion
	}
}