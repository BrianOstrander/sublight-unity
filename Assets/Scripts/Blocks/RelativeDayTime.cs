using System;

namespace LunraGames.SubLight
{
	[Serializable]
	public struct RelativeDayTime
	{
		public static RelativeDayTime Zero
		{
			get
			{
				return new RelativeDayTime
				{
					ShipTime = DayTime.Zero,
					GalacticTime = DayTime.Zero
				};
			}
		}

		public DayTime ShipTime;
		public DayTime GalacticTime;
	}
}