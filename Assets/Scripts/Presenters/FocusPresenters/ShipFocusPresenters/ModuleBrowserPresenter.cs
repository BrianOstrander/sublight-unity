using System.Linq;
using System.Collections.Generic;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ModuleBrowserPresenter : ShipFocusPresenter<IModuleBrowserView>
	{
		GameModel model;
		ModuleBrowserLanguageBlock language;

		protected override bool CanShow()
		{
			switch (model.Context.EncounterState.Current.Value.State)
			{
				case EncounterStateModel.States.Processing:
					return !model.Context.EncounterState.KeyValues.Get(KeyDefines.Encounter.DisableDefaultShipView);
				default:
					return true;
			}
		}
		
		public ModuleBrowserPresenter(
			GameModel model,
			ModuleBrowserLanguageBlock language
		)
		{
			this.model = model;
			this.language = language;

			model.Ship.Statistics.Changed += OnStatistics;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
			
			model.Ship.Statistics.Changed -= OnStatistics;
		}
		
		protected override void OnUpdateEnabled()
		{
			// Debug.Log("uhhh browser enabled...");
			UpdateStatistics(model.Ship.Statistics.Value);
		}

		void UpdateStatistics(ShipStatistics shipStatistics)
		{
			var entries = new List<ModuleBrowserBlock.ModuleEntry>();

			foreach (var module in shipStatistics.Modules)
			{
				var current = new ModuleBrowserBlock.ModuleEntry();
				current.Id = module.Id.Value;
				current.Name = module.Name.Value;
				current.Type = module.Type.Value.ToString();
				current.YearManufacturedTitle = language.YearManufactured.Value;
				current.YearManufactured = module.YearManufactured.Value;
				current.PowerProductionTitle = language.PowerProduction.Value;
				current.PowerProduction = module.PowerProduction.Value.ToString("N2");
				current.PowerConsumptionTitle = language.PowerConsumption.Value;
				current.PowerConsumption = module.PowerConsumption.Value.ToString("N2");
				current.TransitVelocityTitle = language.Velocity.Value;
				current.TransitVelocity = module.TransitVelocity.Value.ToString("N2");
				current.TransitRangeTitle = language.NavigationRange.Value;
				current.TransitRange = UniversePosition.ToLightYearDistance(module.TransitRange.Value).ToString("N2");
				
				current.Description = module.Description;

				var moduleTraits = model.Context.ModuleService.GetTraits(module.TraitIds.Value);
				var definingTrait = moduleTraits.HighestSeverity().FirstOrDefault();

				current.DefiningSeverity = definingTrait?.Severity.Value ?? ModuleTraitSeverity.Neutral;
				current.DefiningSeverityText = current.DefiningSeverity.ToString();

				var currentTraits = new List<ModuleBrowserBlock.ModuleEntry.TraitBlock>();

				foreach (var trait in moduleTraits)
				{
					var currentTrait = new ModuleBrowserBlock.ModuleEntry.TraitBlock();
					currentTrait.Name = trait.Name.Value;
					currentTrait.Description = trait.Description.Value;
					
					currentTrait.SeverityText = language.Severities[trait.Severity.Value].Value;
					currentTrait.Severity = trait.Severity.Value;
					
					currentTraits.Add(currentTrait);
				}

				current.Traits = currentTraits.ToArray();
				
				entries.Add(current);
			}

			View.Info = new ModuleBrowserBlock
			{
				Modules = entries.ToArray(),
				StatsTitle = language.StatsTitle.Value,
				PowerProductionTitle = language.PowerProduction.Value,
				PowerProduction = shipStatistics.PowerProduction.ToString("N2"),
				PowerConsumptionTitle = language.PowerConsumption.Value,
				PowerConsumption = shipStatistics.PowerConsumption.ToString("N2"),
				TransitVelocityTitle = language.Velocity.Value,
				TransitVelocity = shipStatistics.TransitVelocity + language.VelocityUnit.Value,
				TransitRangeTitle = language.NavigationRange.Value,
				TransitRange = shipStatistics.TransitRange + " " + language.NavigationRangeUnit.Value
			};
			
			View.Selected = entries.FirstOrDefault().Id;
			View.Selection = OnSelection;
		}

		#region Model Events
		void OnStatistics(ShipStatistics shipStatistics)
		{
			if (View.Visible) UpdateStatistics(shipStatistics);
		}
		#endregion

		#region View Events
		void OnSelection(string id)
		{
			
		}
		#endregion
	}
}