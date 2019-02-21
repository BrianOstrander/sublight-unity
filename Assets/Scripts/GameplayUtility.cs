using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public static class GameplayUtility
	{
		public static void ApplyTransit(
			float duration,
			KeyValueListModel gameSource
		)
		{
			var rations = gameSource.Get(KeyDefines.Game.Rations);
			var rationsConsumptionMultiplier = gameSource.Get(KeyDefines.Game.RationsConsumptionMultiplier);
			var rationing = gameSource.Get(KeyDefines.Game.Rationing);

			var transitsWithoutRations = gameSource.Get(KeyDefines.Game.TransitsWithoutRations);
			var transitsWithOverPopulation = gameSource.Get(KeyDefines.Game.TransitsWithOverPopulation);
			var transitsWithUnderPopulation = gameSource.Get(KeyDefines.Game.TransitsWithUnderPopulation);

			var population = gameSource.Get(KeyDefines.Game.Population);
			var shipPopulationMaximum = gameSource.Get(KeyDefines.Game.ShipPopulationMaximum);
			var shipPopulationMinimum = gameSource.Get(KeyDefines.Game.ShipPopulationMinimum);

			var populationMinimum = gameSource.Get(KeyDefines.Game.PopulationMinimum);
			var populationMaximum = gameSource.Get(KeyDefines.Game.PopulationMaximumMultiplier) * shipPopulationMaximum;

			var rationsConsumed = population * rationsConsumptionMultiplier * duration;

			if (rationing < 0) rationsConsumed = rationsConsumed / (Mathf.Abs(rationing) + 1);
			else if (0 < rationing) rationsConsumed *= rationing + 1;

			var isStarving = rations < rationsConsumed;
			rations = Mathf.Max(0f, rations - rationsConsumed);

			var populationDelta = gameSource.Get(KeyDefines.Game.PopulationRationingMultiplier);
			populationDelta *= isStarving ? gameSource.Get(KeyDefines.Game.RationingMinimum) : rationing;

			population = Mathf.Clamp(population + populationDelta, populationMinimum, populationMaximum);

			if (isStarving) transitsWithoutRations = Mathf.Min(transitsWithoutRations + 1, gameSource.Get(KeyDefines.Game.TransitsWithoutRationsMaximum));
			if (shipPopulationMaximum < population) transitsWithOverPopulation = Mathf.Min(transitsWithOverPopulation + 1, gameSource.Get(KeyDefines.Game.TransitsWithOverPopulationMaximum));
			if (population < shipPopulationMinimum) transitsWithUnderPopulation = Mathf.Min(transitsWithUnderPopulation + 1, gameSource.Get(KeyDefines.Game.TransitsWithUnderPopulationMaximum));

			var propellant = gameSource.Get(KeyDefines.Game.Propellant);
			var propellantUsage = gameSource.Get(KeyDefines.Game.PropellantUsage) + 1;

			propellant = propellant - propellantUsage;
			propellantUsage = Mathf.Max(0, Mathf.Min(propellant, propellantUsage - 1));

			gameSource.Set(
				KeyDefines.Game.Rations,
				rations
			);

			gameSource.Set(
				KeyDefines.Game.TransitsWithoutRations,
				transitsWithoutRations
			);

			gameSource.Set(
				KeyDefines.Game.TransitsWithOverPopulation,
				transitsWithOverPopulation
			);

			gameSource.Set(
				KeyDefines.Game.TransitsWithUnderPopulation,
				transitsWithUnderPopulation
			);

			gameSource.Set(
				KeyDefines.Game.Population,
				population
			);

			gameSource.Set(
				KeyDefines.Game.Propellant,
				propellant
			);

			gameSource.Set(
				KeyDefines.Game.PropellantUsage,
				propellantUsage
			);
		}

		//public static void VelocityByEnergyMultiplier(
		//	float velocityBaseLightYear,
		//	int count,
		//	out float relativeVelocity,
		//	out float newtonianVelocity
		//)
		//{
		//	if (count < 1) throw new ArgumentOutOfRangeException("count", "Needs to be at least 1");

		//	var baseEnergy = (1f / Mathf.Sqrt(1f - Mathf.Pow(velocityBaseLightYear, 2f))) - 1f;
		//	var finalEnergy = baseEnergy * count;
		//	relativeVelocity = Mathf.Sqrt(1f - Mathf.Pow((1f / (finalEnergy + 1f)), 2f));
		//	newtonianVelocity = velocityBaseLightYear * Mathf.Sqrt(count);
		//}

		//public static RelativeDayTime TransitTime(
		//	float velocityLightYear,
		//	float distanceLightYear
		//)
		//{
		//	var galacticTime = distanceLightYear / velocityLightYear;
		//	var shipTime = galacticTime * (1f / (1f / Mathf.Sqrt(1f - (Mathf.Pow(velocityLightYear, 2f) / 1f))));

		//	return new RelativeDayTime(
		//		DayTime.FromYear(shipTime),
		//		DayTime.FromYear(galacticTime)
		//	);
		//}
	}
}