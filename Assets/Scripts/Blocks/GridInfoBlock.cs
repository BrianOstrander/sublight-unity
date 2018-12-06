using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct GridInfoBlock
	{
		public LanguageStringModel Scale;
		public UniverseScales[] Scales;
		public LanguageStringModel[] ScaleNames;

		public LanguageStringModel ThousandUnit;
		public LanguageStringModel MillionUnit;

		public PluralLanguageStringBlock AstronomicalUnit;
		public PluralLanguageStringBlock LightYearUnit;

		public void GetScaleName(float zoom, out UniverseScales scale, out LanguageStringModel scaleName)
		{
			var zoomIndex = Mathf.FloorToInt(zoom);
			scale = Scales[zoomIndex];
			scaleName = ScaleNames[zoomIndex];
		}

		public void GetUnitModels(
			float count,
			PluralLanguageStringBlock unitTypePlural,
			out Func<string> getUnitCount,
			out LanguageStringModel unitType
		)
		{
			unitType = unitTypePlural.Get(count);
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