using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct ZoomInfoBlock
	{
		public float Zoom;
		public float Clustering;
		public int ScaleIndex;
		public GridUnitTypes UnitType;
		public float UnitAmount;
		public LanguageStringModel ScaleName;
		public LanguageStringModel UnitName;
		public Func<string> UnitAmountFormatted;
	}
}