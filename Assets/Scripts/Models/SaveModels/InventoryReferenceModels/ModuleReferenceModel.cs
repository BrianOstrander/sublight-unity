using LunraGames.NumberDemon;

namespace LunraGames.SpaceFarm.Models
{
	public class ModuleReferenceModel : InventoryReferenceModel<ModuleInventoryModel>
	{
		public ModuleReferenceModel() : base(SaveTypes.ModuleReference) {}

		protected override void OnInitializeInstance(InventoryReferenceContext context)
		{
			var model = Model.Value;

			model.AcquiredDate.Value = context.CurrentDate;
			var functionalDelta = model.DeltaLifespan.Value * model.CurveLifespan.Value.Evaluate(DemonUtility.NextFloat);
			model.Lifespan.Value = model.MinimumLifespan.Value + functionalDelta;
			model.FailureDate.Value = model.AcquiredDate.Value + model.Lifespan.Value;

			// TODO: Perhaps this should be on a curve to make accurate values less likely?
			model.EstimatedFailureRange.Value = DemonUtility.NextFloat;

			var totalEstimateRange = 1f - (context.EstimateFailureRange * model.EstimatedFailureRange.Value);

			model.EstimatedFailureDate.Value = model.AcquiredDate.Value + (model.Lifespan.Value * totalEstimateRange);
		}
	}
}