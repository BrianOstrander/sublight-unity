using System;

using UnityEngine;

using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public static class GameplayUtility
	{
		static float RationsConsumed(
			float duration,
			float population,
			float rationsConsumptionMultiplier,
			int rationing
		)
		{
			var rationsConsumed = population * rationsConsumptionMultiplier * duration;

			if (rationing < 0) rationsConsumed = rationsConsumed / (Mathf.Abs(rationing) + 1);
			else if (0 < rationing) rationsConsumed *= rationing + 1;

			return rationsConsumed;
		}

		public static void RationsAvailable(
			KeyValueListModel gameSource,
			KeyValueListModel systemSource,
			out float rationsTotal
		)
		{
			float rationsFromSystem;
			RationsAvailable(
				gameSource,
				systemSource,
				out rationsTotal,
				out rationsFromSystem
			);
		}

		public static void RationsAvailable(
			KeyValueListModel gameSource,
			KeyValueListModel systemSource,
			out float rationsTotal,
			out float rationsFromSystem
		)
		{
			if (gameSource == null) throw new ArgumentNullException("gameSource");
			if (systemSource == null) throw new ArgumentNullException("systemSource");

			rationsFromSystem = systemSource.Get(KeyDefines.CelestialSystem.RemainingRations);
			rationsTotal = gameSource.Get(KeyDefines.Game.Rations) + rationsFromSystem;
		}

		public static bool RationingValidation(
			float duration,
			KeyValueListModel gameSource,
			KeyValueListModel systemSource,
			out int rationingMinimum,
			out int rationingMaximum,
			out int rationingLimit,
			out bool rationsInsufficientForLimit
		)
		{
			if (gameSource == null) throw new ArgumentNullException("gameSource");
			if (systemSource == null) throw new ArgumentNullException("systemSource");

			rationingMinimum = gameSource.Get(KeyDefines.Game.RationingMinimum);
			rationingMaximum = gameSource.Get(KeyDefines.Game.RationingMaximum);
			rationingLimit = rationingMinimum;
			rationsInsufficientForLimit = true;

			var rationingDelta = (rationingMaximum - rationingMinimum) + 1;

			// TODO: Maybe put these somewhere... in like a constant or something?
			if (rationingDelta < 3 || 17 < rationingDelta) return false;

			float rationsTotal;
			RationsAvailable(gameSource, systemSource, out rationsTotal);

			var population = gameSource.Get(KeyDefines.Game.Population);
			var rationsConsumptionMultiplier = gameSource.Get(KeyDefines.Game.RationsConsumptionMultiplier);

			for (var i = rationingMinimum; i <= rationingMaximum; i++)
			{
				if (rationsTotal < RationsConsumed(duration, population, rationsConsumptionMultiplier, i)) break;
				rationingLimit = i;
				rationsInsufficientForLimit = false;
			}

			return true;
		}

		public static void ApplyTransit(
			float duration,
			KeyValueListModel gameSource,
			KeyValueListModel systemSource
		)
		{
			if (gameSource == null) throw new ArgumentNullException("gameSource");
			if (systemSource == null) throw new ArgumentNullException("systemSource");

			// -- To Update
			var transitsWithoutRations = gameSource.Get(KeyDefines.Game.TransitsWithoutRations);
			var transitsWithOverPopulation = gameSource.Get(KeyDefines.Game.TransitsWithOverPopulation);
			var transitsWithUnderPopulation = gameSource.Get(KeyDefines.Game.TransitsWithUnderPopulation);

			var population = gameSource.Get(KeyDefines.Game.Population);
			var rationing = gameSource.Get(KeyDefines.Game.Rationing);
			float? rations = null;
			float? rationsRemainingInSystem = null;

			var propellant = gameSource.Get(KeyDefines.Game.Propellant);
			var propellantUsage = gameSource.Get(KeyDefines.Game.PropellantUsage) + 1; // Indexing of velocities starts at zero, but we always want to consume full.
			// --

			int rationingMinimum;
			int rationingMaximum;
			int rationingLimit;
			bool rationsInsufficientForLimit;

			if (RationingValidation(
				duration,
				gameSource,
				systemSource,
				out rationingMinimum,
				out rationingMaximum,
				out rationingLimit,
				out rationsInsufficientForLimit
			))
			{
				var shipPopulationMaximum = gameSource.Get(KeyDefines.Game.ShipPopulationMaximum);
				var shipPopulationMinimum = gameSource.Get(KeyDefines.Game.ShipPopulationMinimum);
				var populationMinimum = gameSource.Get(KeyDefines.Game.PopulationMinimum);
				var populationMaximum = gameSource.Get(KeyDefines.Game.PopulationMaximumMultiplier) * shipPopulationMaximum;
				var populationRationingMultiplier = gameSource.Get(KeyDefines.Game.PopulationRationingMultiplier);
				var rationsConsumptionMultiplier = gameSource.Get(KeyDefines.Game.RationsConsumptionMultiplier);

				// Rationing is valid and we can make calculations based on it.
				rationing = Mathf.Min(rationing, rationingLimit);
				var rationsConsumed = RationsConsumed(
					duration,
					population,
					rationsConsumptionMultiplier,
					rationing
				);


				float rationsTotal;
				float rationsFromSystem;
				RationsAvailable(
					gameSource,
					systemSource,
					out rationsTotal,
					out rationsFromSystem
				);

				rationsTotal = Mathf.Max(0f, rationsTotal - rationsConsumed);
				var rationsMaximum = gameSource.Get(KeyDefines.Game.RationsMaximum);

				rations = Mathf.Min(rationsMaximum, rationsTotal);
				rationsRemainingInSystem = Mathf.Min(Mathf.Max(0f, rationsTotal - rations.Value), rationsFromSystem);

				population = Mathf.Clamp(
					population + ((rationsInsufficientForLimit ? rationingMinimum : rationing) * populationRationingMultiplier),
					populationMinimum,
					populationMaximum
				);

				if (rationsInsufficientForLimit) transitsWithoutRations = Mathf.Min(transitsWithoutRations + 1, gameSource.Get(KeyDefines.Game.TransitsWithoutRationsMaximum));
				if (shipPopulationMaximum < population) transitsWithOverPopulation = Mathf.Min(transitsWithOverPopulation + 1, gameSource.Get(KeyDefines.Game.TransitsWithOverPopulationMaximum));
				if (population < shipPopulationMinimum) transitsWithUnderPopulation = Mathf.Min(transitsWithUnderPopulation + 1, gameSource.Get(KeyDefines.Game.TransitsWithUnderPopulationMaximum));
			}
			else
			{
				Debug.LogError(
					"Invalid rationing range [ " + rationingMinimum + " , " + rationingMaximum + " ]" +
	               	"\nThis really shouldn't happen, make sure you initialized the rules properly"
				);
			}

			propellant = propellant - propellantUsage;
			propellantUsage = Mathf.Max(0, Mathf.Min(propellant, propellantUsage - 1));

			// -- Updating
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
				KeyDefines.Game.Rationing,
				rationing
			);

			if (rations.HasValue)
			{
				gameSource.Set(
					KeyDefines.Game.Rations,
					rations.Value
				);
			}

			if (rationsRemainingInSystem.HasValue)
			{
				systemSource.Set(
					KeyDefines.CelestialSystem.RemainingRations,
					rationsRemainingInSystem.Value
				);
			}

			// --

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