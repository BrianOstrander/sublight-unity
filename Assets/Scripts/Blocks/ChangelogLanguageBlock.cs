using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct ChangelogLanguageBlock
	{
		public LanguageStringModel Title;
		public LanguageStringModel Back;

		public LanguageStringModel ChangeFeature;
		public LanguageStringModel ChangeImprovement;
		public LanguageStringModel ChangeFix;
		public LanguageStringModel ChangeDeprecation;

		public Func<int, int, int, string> GetDate;
	}
}