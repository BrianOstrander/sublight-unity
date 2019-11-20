using System.Collections.Generic;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct ModuleSwapLanguageBlock
	{
		public Dictionary<ModuleSwapEncounterLogModel.Styles, LanguageStringModel> AvailableSourceTypeDefaults;
		public LanguageStringModel CurrentType;
		public LanguageStringModel RemovedType;
		
		public LanguageStringModel Confirm;
		
		public LanguageStringModel Velocity;
		public LanguageStringModel VelocityUnit;
		public LanguageStringModel NavigationRange;
		public LanguageStringModel NavigationRangeUnit;
		public LanguageStringModel YearManufactured;
		public LanguageStringModel PowerProduction;
		public LanguageStringModel PowerConsumption;

		public Dictionary<ModuleTraitSeverity, LanguageStringModel> Severities;
		public Dictionary<ModuleTypes, LanguageStringModel> Types;
	}
}