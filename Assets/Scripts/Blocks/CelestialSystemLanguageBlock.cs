using System.Collections.Generic;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct CelestialSystemLanguageBlock
	{
		public LanguageStringModel Confirm;
		public LanguageStringModel ConfirmDescription;
		public LanguageStringModel OutOfRange;
		public LanguageStringModel OutOfRangeDescription;
		public LanguageStringModel DistanceUnit;
		public LanguageStringModel Analysis;
		public LanguageStringModel AnalysisDescription;

		public Dictionary<SystemClassifications, LanguageStringModel> PrimaryClassifications;
	}
}