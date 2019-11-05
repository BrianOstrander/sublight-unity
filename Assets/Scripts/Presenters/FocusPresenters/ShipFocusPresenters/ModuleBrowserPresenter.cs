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
		
		public ModuleBrowserPresenter(
			GameModel model,
			ModuleBrowserLanguageBlock language
		)
		{
			this.model = model;
			this.language = language;
			
			model.Ship.Modules.Changed += OnModules;
		}

		protected override void OnUnBind()
		{
			base.OnUnBind();
			
			model.Ship.Modules.Changed -= OnModules;
		}
		
		protected override void OnUpdateEnabled()
		{
			// Debug.Log("uhhh browser enabled...");
			UpdateModules(model.Ship.Modules.Value);
		}

		void UpdateModules(ModuleStatistics moduleStatistics)
		{
			var entries = new List<ModuleBrowserBlock.ModuleEntry>();

			foreach (var module in moduleStatistics.Modules)
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
				VelocityTitle = language.Velocity.Value,
				VelocityValue = moduleStatistics.NavigationVelocity + language.VelocityUnit.Value,
				NavigationRangeTitle = language.NavigationRange.Value,
				NavigationRangeValue = moduleStatistics.NavigationRange + " " + language.NavigationRangeUnit.Value
			};
			
			View.Selected = entries.FirstOrDefault().Id;
			View.Selection = OnSelection;
		}
		
		#region Model Events
		void OnModules(ModuleStatistics moduleStatistics)
		{
			if (View.Visible) UpdateModules(moduleStatistics);
		}
		#endregion

		#region View Events
		void OnSelection(string id)
		{
			
		}
		#endregion
	}
}