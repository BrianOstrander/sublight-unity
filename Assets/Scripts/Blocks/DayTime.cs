using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct DayTime
	{
		public const float TimeInDay = 60f;

		public static DayTime Zero { get { return new DayTime(); } }

		public int Day;
		public float Time;

		public bool Equals(DayTime dayTime)
		{
			return Day == dayTime.Day && Mathf.Approximately(Time, dayTime.Time);
		}
	}
}