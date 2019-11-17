using System.Collections.Generic;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct ModuleSwapLanguageBlock
	{
		public LanguageStringModel Velocity;
		public LanguageStringModel VelocityUnit;
		public LanguageStringModel NavigationRange;
		public LanguageStringModel NavigationRangeUnit;
		public LanguageStringModel YearManufactured;
		public LanguageStringModel PowerProduction;
		public LanguageStringModel PowerConsumption;

		public Dictionary<ModuleTraitSeverity, LanguageStringModel> Severities;
	}
}