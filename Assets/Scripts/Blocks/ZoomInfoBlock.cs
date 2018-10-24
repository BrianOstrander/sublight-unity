using System;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public struct ZoomInfoBlock
	{
		public float Zoom;
		public float Clustering; // possibly delete
		public int ScaleIndex;
		public GridUnitTypes UnitType;
		public float UnitAmountMinimum;
		public float UnitAmountMaximum;
		public float UnitProgress;
		public LanguageStringModel ScaleName;
		public LanguageStringModel UnitName;
		public Func<string> UnitAmountFormatted;
	}
}