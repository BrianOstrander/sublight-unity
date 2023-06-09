﻿using System;

using UnityEngine;

namespace LunraGames.SubLight
{
	public static class RelativityUtility
	{
		public static void VelocityByEnergyMultiplier(
			float velocityBaseLightYear,
			int count,
			out float relativeVelocity,
			out float newtonianVelocity
		)
		{
			if (count < 1) throw new ArgumentOutOfRangeException("count", "Needs to be at least 1");

			var baseEnergy = (1f / Mathf.Sqrt(1f - Mathf.Pow(velocityBaseLightYear, 2f))) - 1f;
			var finalEnergy = baseEnergy * count;
			relativeVelocity = Mathf.Sqrt(1f - Mathf.Pow((1f / (finalEnergy + 1f)), 2f));
			newtonianVelocity = velocityBaseLightYear * Mathf.Sqrt(count);
		}

		public static RelativeDayTime TransitTime(
			float velocityLightYear,
			float distanceLightYear
		)
		{
			var galacticTime = distanceLightYear / velocityLightYear;
			var shipTime = galacticTime * (1f /(1f / Mathf.Sqrt(1f - (Mathf.Pow(velocityLightYear, 2f) / 1f))));

			return new RelativeDayTime(
				DayTime.FromYear(shipTime),
				DayTime.FromYear(galacticTime)
			);
		}
	}
}