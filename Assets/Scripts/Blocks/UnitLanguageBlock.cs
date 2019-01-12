using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct UnitLanguageBlock
	{
		public LanguageStringModel ThousandUnit;
		public LanguageStringModel MillionUnit;

		public PluralLanguageStringBlock Unit;

		public void GetUnitModels(
			float count,
			out Func<string> getUnitCount,
			out LanguageStringModel unitType
		)
		{
			unitType = Unit.Get(count);
			float expressed;
			LanguageStringModel suffix;
			ReasonableUnits(count, out expressed, out suffix);

			getUnitCount = () =>
			{
				return suffix == null ? expressed.ToString("N0") : expressed.ToString("N0") + suffix.Value.Value;
			};
		}

		void ReasonableUnits(float count, out float expressed, out LanguageStringModel suffix)
		{
			expressed = count;
			suffix = null;

			if (1000000f <= count) // Million
			{
				expressed = count / 1000000;
				suffix = MillionUnit;
				return;
			}
			if (1000 <= count) // Thousand
			{
				expressed = count / 1000;
				suffix = ThousandUnit;
			}
		}
	}
}