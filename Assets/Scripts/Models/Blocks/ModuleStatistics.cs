using System;

namespace LunraGames.SubLight.Models
{
	[Serializable]
	public struct ModuleStatistics
	{
		public static ModuleStatistics Default => new ModuleStatistics(new ModuleModel[0]);
		
		public readonly ModuleModel[] Modules;
		public readonly float NavigationVelocity;
		public readonly float NavigationRange;
		public readonly float PowerConsumption;
		public readonly float PowerProduction;

		public ModuleStatistics(
			ModuleModel[] modules
		)
		{
			Modules = modules ?? throw new ArgumentNullException(nameof(modules));

			NavigationVelocity = 0f;
			NavigationRange = 0f;
			PowerConsumption = 0f;
			PowerProduction = 0f;
			
			foreach (var module in modules)
			{
				NavigationVelocity += module.TransitVelocity.Value;
				NavigationRange += module.TransitRange.Value;
				PowerConsumption += module.PowerConsumption.Value;
				PowerProduction += module.PowerProduction.Value;
			}
		}
	}
}