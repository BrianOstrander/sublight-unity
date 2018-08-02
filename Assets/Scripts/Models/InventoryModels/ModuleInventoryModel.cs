using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ModuleInventoryModel : InventoryModel
	{
		[JsonProperty] bool isRoot;
		[JsonProperty] DayTime minimumLifespan;
		[JsonProperty] DayTime deltaLifespan;
		[JsonProperty] AnimationCurve curveLifespan = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[JsonProperty] DayTime acquiredDate;
		[JsonProperty] DayTime lifespan;
		[JsonProperty] DayTime failureDate;
		[JsonProperty] float estimatedFailureRange;
		[JsonProperty] DayTime estimatedFailureDate;

		[JsonProperty] ModuleSlotListModel slots = new ModuleSlotListModel();

		[JsonIgnore]
		public readonly ListenerProperty<bool> IsRoot;
		[JsonIgnore]
		public readonly ListenerProperty<DayTime> MinimumLifespan;
		[JsonIgnore]
		public readonly ListenerProperty<DayTime> DeltaLifespan;
		[JsonIgnore]
		public readonly ListenerProperty<AnimationCurve> CurveLifespan;

		#region Assigned on Instantiation
		[JsonIgnore]
		public readonly ListenerProperty<DayTime> AcquiredDate;
		[JsonIgnore]
		public readonly ListenerProperty<DayTime> Lifespan;
		/// <summary>
		/// The actual date the module will fail.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<DayTime> FailureDate;
		/// <summary>
		/// The base failure estimate range, a value from 0 to 1. Lower values
		/// indicate a more accurate base estimate, before modifiers from the
		/// player's ship.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> EstimatedFailureRange;
		/// <summary>
		/// The date we tell the player the module is going to fail, not
		/// neccessarily the actual date.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<DayTime> EstimatedFailureDate;
		#endregion

		[JsonIgnore]
		public ModuleSlotListModel Slots { get { return slots; } }

		public override InventoryTypes InventoryType { get { return InventoryTypes.Module; } }
		public override bool IsUsable { get { return base.IsUsable || IsRoot.Value; } }

		public bool IsEstimatedFunctional(DayTime current) { return current < EstimatedFailureDate.Value; }
		public bool IsFunctional(DayTime current) { return current < FailureDate.Value; }

		public ModuleInventoryModel()
		{
			IsRoot = new ListenerProperty<bool>(value => isRoot = value, () => isRoot);
			MinimumLifespan = new ListenerProperty<DayTime>(value => minimumLifespan = value, () => minimumLifespan);
			DeltaLifespan = new ListenerProperty<DayTime>(value => deltaLifespan = value, () => deltaLifespan);
			CurveLifespan = new ListenerProperty<AnimationCurve>(value => curveLifespan = value, () => curveLifespan);

			AcquiredDate = new ListenerProperty<DayTime>(value => acquiredDate = value, () => acquiredDate);
			Lifespan = new ListenerProperty<DayTime>(value => lifespan = value, () => lifespan);
			FailureDate = new ListenerProperty<DayTime>(value => failureDate = value, () => failureDate);
			EstimatedFailureRange = new ListenerProperty<float>(value => estimatedFailureRange = value, () => estimatedFailureRange);
			EstimatedFailureDate = new ListenerProperty<DayTime>(value => estimatedFailureDate = value, () => estimatedFailureDate);

			InstanceId.Changed += OnInstanceId;
		}

		#region Events
		void OnInstanceId(string newInstanceId)
		{
			slots.ParentId.Value = newInstanceId;
		}
		#endregion
	}
}