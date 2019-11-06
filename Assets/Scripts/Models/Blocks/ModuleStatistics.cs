using System;

namespace LunraGames.SubLight.Models
{
	[Serializable]
	public struct ModuleStatistics
	{
		public static ModuleStatistics Default => new ModuleStatistics(new ModuleModel[0]);
		
		public readonly ModuleModel[] Modules;
		public readonly float TransitVelocity;
		public readonly float TransitRange;
		public readonly float PowerConsumption;
		public readonly float PowerProduction;

		public ModuleStatistics(
			ModuleModel[] modules
		)
		{
			Modules = modules ?? throw new ArgumentNullException(nameof(modules));

			TransitVelocity = 0f;
			TransitRange = 0f;
			PowerConsumption = 0f;
			PowerProduction = 0f;
			
			foreach (var module in modules)
			{
				TransitVelocity += module.TransitVelocity.Value;
				TransitRange += module.TransitRange.Value;
				PowerConsumption += module.PowerConsumption.Value;
				PowerProduction += module.PowerProduction.Value;
			}
		}
	}
}