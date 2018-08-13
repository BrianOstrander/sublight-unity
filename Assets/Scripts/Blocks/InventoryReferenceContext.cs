using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	/// <summary>
	/// All the context that is needed for created an instance of any
	/// InventoryReference.
	/// </summary>
	/// <remarks>
	/// This basically exists so InventoryReferences can ignore values they
	/// don't care about without having to add them as parameters to their
	/// initialization method.
	/// </remarks>
	public struct InventoryReferenceContext
	{
		public static InventoryReferenceContext Default
		{
			get
			{
				return new InventoryReferenceContext(
					DayTime.Zero,
					0.1f
				);
			}
		}

		public static InventoryReferenceContext Current(
			GameModel model
		)
		{
			return new InventoryReferenceContext(
				model.DayTime,
				Mathf.Clamp01(1f - model.Ship.Value.Inventory.MaximumResources.EstimateFailureRange.Value)
			);
		}

		public readonly DayTime CurrentDate;
		/// <summary>
		/// A value from 0 to 1 that is multiplied by the module's failure
		/// range, making this value the maximum margin of error for estimating
		/// a module's lifespan. Lower values result in better estimates.
		/// </summary>
		public readonly float EstimateFailureRange;

		public InventoryReferenceContext(
			DayTime currentDate,
			float estimateFailureRange
		)
		{
			CurrentDate = currentDate;
			EstimateFailureRange = estimateFailureRange;
		}
	}
}