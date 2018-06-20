using System;

using Newtonsoft.Json;

using UnityEngine;

namespace LunraGames.SpaceFarm
{
	[Serializable]
	public struct DayTime
	{
		public const float DaysInYear = 365;
		public const float TimeInDay = 4f;

		public static DayTime Zero { get { return new DayTime(); } }

		public static DayTime FromDayNormal(float dayNormal) { return new DayTime(dayNormal * TimeInDay); }

		[JsonProperty] public readonly int Day;
		[JsonProperty] public readonly float Time;

		/// <summary>
		/// Gets the total time.
		/// </summary>
		/// <value>The total time.</value>
		[JsonIgnore]
		public float TotalTime { get { return (Day * TimeInDay) + Time; } }
		/// <summary>
		/// Gets the total time represented by this DayTime where a Day is equal to 1.0
		/// </summary>
		/// <value>The day normal.</value>
		[JsonIgnore]
		public float DayNormal { get { return TotalTime / TimeInDay; } }
		/// <summary>
		/// Gets the years.
		/// </summary>
		/// <value>The years.</value>
		[JsonIgnore]
		public float Years { get { return Day / DaysInYear; } }
		/// <summary>
		/// Gets the DayTime value floored to the nearest day.
		/// </summary>
		/// <value>The day floor.</value>
		[JsonIgnore]
		public int DayFloor { get { return Day; } }
		/// <summary>
		/// Gets the DayTime value ceiling to the nearest day.
		/// </summary>
		/// <value>The day ceiling.</value>
		[JsonIgnore]
		public int DayCeiling { get { return Mathf.Approximately(0f, Time) ? Day : Day + 1; } }

		public DayTime(float time)
		{
			var newTime = time % TimeInDay;
			var dayTime = time - newTime;
			Day = Mathf.FloorToInt(dayTime / TimeInDay);
			Time = newTime;
		}

		public DayTime(int day)
		{
			Day = day;
			Time = 0f;
		}

		public DayTime(int day, float time) : this(time)
		{
			Day += day;
		}

		/// <summary>
		/// Add the specified day and time and returns a new DayTime object containing the result.
		/// </summary>
		/// <returns>The result of the addition.</returns>
		/// <param name="day">Day.</param>
		/// <param name="time">Time.</param>
		public DayTime Add(int day, float time)
		{
			var dayResult = Day;
			var timeResult = Time;
			time += timeResult;
			var newTime = time % TimeInDay;
			var dayTime = time - newTime;
			dayResult += day + Mathf.FloorToInt(dayTime / TimeInDay);
			timeResult = newTime;
			return new DayTime(dayResult, timeResult);
		}

		public static DayTime Max(DayTime dayTime0, DayTime dayTime1)
		{
			if (dayTime0.Day < dayTime1.Day) return dayTime1;
			if (dayTime1.Day < dayTime0.Day) return dayTime0;
			if (dayTime0.Time < dayTime1.Time) return dayTime1;
			return dayTime0;
		}

		public static DayTime Min(DayTime dayTime0, DayTime dayTime1)
		{
			if (dayTime0.Day < dayTime1.Day) return dayTime0;
			if (dayTime1.Day < dayTime0.Day) return dayTime1;
			if (dayTime0.Time < dayTime1.Time) return dayTime0;
			return dayTime1;
		}

		public static DayTime DayTimeElapsed(DayTime dayTime0, DayTime dayTime1)
		{
			var max = Max(dayTime0, dayTime1);
			var min = Min(dayTime0, dayTime1);
			var day = max.Day - min.Day;
			var time = max.Time - min.Time;
			if (time < 0f)
			{
				time = TimeInDay + time;
				day--;
			}
			return new DayTime(day, time);
		}

		public static DayTime operator +(DayTime obj0, DayTime obj1)
		{
			return obj0.Add(obj1.Day, obj1.Time);
		}

		public override string ToString()
		{
			return "TotalTime: " + TotalTime;
		}
	}
}