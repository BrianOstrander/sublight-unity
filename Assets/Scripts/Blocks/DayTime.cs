﻿using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public struct DayTime
	{
		public const float TimeInDay = 60f;

		public static DayTime Zero { get { return new DayTime(); } }

		public int Day;
		public float Time;

		public float TotalTime { get { return (Day * TimeInDay) + Time; } }

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
			var result = new DayTime(Day, Time);
			time += result.Time;
			var newTime = time % TimeInDay;
			var dayTime = time - newTime;
			result.Day = day + Mathf.FloorToInt(dayTime / TimeInDay);
			result.Time = newTime;
			return result;
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
			if (time < 0f) time = TimeInDay + time;
			return new DayTime(day, time);
		}
	}
}