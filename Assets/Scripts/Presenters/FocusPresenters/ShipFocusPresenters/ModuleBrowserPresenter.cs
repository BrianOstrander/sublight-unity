using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.SubLight.Models;
using LunraGames.SubLight.Views;

namespace LunraGames.SubLight.Presenters
{
	public class ModuleBrowserPresenter : ShipFocusPresenter<IModuleBrowserView>
	{
		GameModel model;
		
		public ModuleBrowserPresenter(
			GameModel model
		)
		{
			this.model = model;
			
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

		void UpdateModules(ModuleModel[] modules)
		{
			var entries = new List<ModuleBrowserBlock>();

			foreach (var module in modules)
			{
				var current = new ModuleBrowserBlock();
				current.Id = module.Id.Value;
				current.Name = module.Name.Value;
				current.Type = module.Type.Value.ToString();
				current.YearManufacturedTitle = "Year Manufactured";
				current.YearManufactured = module.YearManufactured.Value;
				current.PowerProductionTitle = "Power Production";
				current.PowerProduction = module.PowerProduction.Value.ToString("N2");
				current.PowerConsumptionTitle = "Power Consumption";
				current.PowerConsumption = module.PowerConsumption.Value.ToString("N2");
				current.Description = module.Description;

				var moduleTraits = model.Context.ModuleService.GetTraits(module.TraitIds.Value);
				var definingTrait = moduleTraits.HighestSeverity().FirstOrDefault();

				current.DefiningSeverity = definingTrait?.Severity.Value ?? ModuleTraitSeverity.Neutral;
				current.DefiningSeverityText = current.DefiningSeverity.ToString();

				var currentTraits = new List<ModuleBrowserBlock.TraitBlock>();

				foreach (var trait in moduleTraits)
				{
					var currentTrait = new ModuleBrowserBlock.TraitBlock();
					currentTrait.Name = trait.Name.Value;
					currentTrait.Description = trait.Description.Value;
					
					currentTrait.SeverityText = trait.Severity.Value.ToString();
					currentTrait.Severity = trait.Severity.Value;
					
					currentTraits.Add(currentTrait);
				}

				current.Traits = currentTraits.ToArray();
				
				entries.Add(current);
			}

			View.Entries = entries.ToArray();
			View.Selected = entries.FirstOrDefault().Id;
			View.Selection = OnSelection;
		}
		
		#region Model Events
		void OnModules(ModuleModel[] modules)
		{
			if (View.Visible) UpdateModules(modules);
		}
		#endregion

		#region View Events
		void OnSelection(string id)
		{
			
		}
		#endregion
	}
}