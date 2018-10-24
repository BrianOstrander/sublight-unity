using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct GridInfoBlock
	{
		public LanguageStringModel Scale;
		public LanguageStringModel[] ScaleNames;

		public LanguageStringModel ThousandUnit;
		public LanguageStringModel MillionUnit;

		public PluralLanguageStringBlock AstronomicalUnit;
		public PluralLanguageStringBlock LightYearUnit;
	}
}