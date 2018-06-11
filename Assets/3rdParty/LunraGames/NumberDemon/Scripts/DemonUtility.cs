using System;
using UnityEngine;
using Random = System.Random;

namespace LunraGames.NumberDemon
{
	public static class DemonUtility
	{
		static Demon Generator = new Demon();

		const int IntHalfValue = (int.MaxValue / 2) + 1;
		const uint UintHalfValue = (uint.MaxValue / 2u) + 1u;
		const ulong UlongHalfValue = (ulong.MaxValue / 2uL) + 1uL;

		/// <summary>
		/// Given a unique pair of integers and their order, return a unique integer. When at least one value is above 1,073,741,824, they roll over. They roll under somewhere too, but I'm too lazy to check.
		/// </summary>
		/// <returns>Returns a unique integer for the given pair.</returns>
		/// <param name="value0">Value0.</param>
		/// <param name="value1">Value1.</param>
		public static int CantorPair(int value0, int value1)
		{
			return ToInt(CantorPair(ToUint(value0), ToUint(value1)));
		}

		/// <summary>
		/// Given a unique pair of unsigned integers and their order, return a unique unsigned integer. High or low values roll over or under, have fun finding out which ones!
		/// </summary>
		/// <returns>Returns a unique unsigned integer for the given pair.</returns>
		/// <param name="value0">Value0.</param>
		/// <param name="value1">Value1.</param>
		public static uint CantorPair(uint value0, uint value1)
		{
			return (((value0 + value1) * (value0 + value1 + 1u)) / 2u) + value1;
		}

		/// <summary>
		/// Given a unique pair of longs and their order, return a unique long. High or low values roll over or under, have fun finding out which ones!
		/// </summary>
		/// <returns>Returns a unique number for the given pair.</returns>
		/// <param name="value0">Value0.</param>
		/// <param name="value1">Value1.</param>
		public static long CantorPair(long value0, long value1)
		{
			return ToLong(CantorPair(ToUlong(value0), ToUlong(value1)));
		}

		/// <summary>
		/// Given a unique pair of unsigned longs and their order, return a unique unsigned long. High or low values roll over or under, have fun finding out which ones!
		/// </summary>
		/// <returns>Returns a unique unsigned long for the given pair.</returns>
		/// <param name="value0">Value0.</param>
		/// <param name="value1">Value1.</param>
		public static ulong CantorPair(ulong value0, ulong value1)
		{
			return (((value0 + value1) * (value0 + value1 + 1uL)) / 2uL) + value1;
		}

		/// <summary>
		/// Takes an integer and maps it to an unsigned integer, where int.MinValue == 0u.
		/// </summary>
		/// <returns>The uint.</returns>
		/// <param name="value">Value.</param>
		public static uint ToUint(int value)
		{
			if (value < 0) return UintHalfValue - ((uint)Math.Abs((long)value));
			else return UintHalfValue + (uint)value;
		}

		/// <summary>
		/// Takes an unsigned integer and maps it to an integer, where int.MinValue == 0u.
		/// </summary>
		/// <returns>The int.</returns>
		/// <param name="value">Value.</param>
		public static int ToInt(uint value)
		{
			if (value < UintHalfValue) return 0 - (int)(UintHalfValue - value);
			else return (int)(value - UintHalfValue);
		}

		/// <summary>
		/// Takes a long and maps it to an unsigned long, where long.MinValue = 0uL.
		/// </summary>
		/// <returns>The ulong.</returns>
		/// <param name="value">Value.</param>
		public static ulong ToUlong(long value)
		{
			if (value < 0L) 
			{
				if (value == long.MinValue) return 0uL;
				else return UlongHalfValue - ((ulong)Math.Abs(value));
			}
			else return UlongHalfValue + (ulong)value;
		}

		/// <summary>
		/// Takes an unsigned long and maps it to a long, where long.MinValue = 0uL.
		/// </summary>
		/// <returns>The long.</returns>
		/// <param name="value">Value.</param>
		public static long ToLong(ulong value)
		{
			if (value < UlongHalfValue) return 0L - (long)(UlongHalfValue - value);
			else return (long)(value - UlongHalfValue);
		}

		public static int NextInteger { get { return Generator.NextInteger; } }
		public static long NextLong { get { return Generator.NextLong; } }
		public static float NextFloat { get { return Generator.NextFloat; } }
		public static Color NextColor { get { return new Color(NextFloat, NextFloat, NextFloat); } }
	}
}