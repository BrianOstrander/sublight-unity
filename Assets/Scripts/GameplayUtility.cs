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

		public static void ResourcesAvailable(
			KeyValueListModel gameSource,
			KeyValueListModel systemSource,
			GameKeys.Resource shipResource,
			CelestialSystemKeys.Resource systemResource,
			out float resourcesTotal
		)
		{
			float resourcesFromSystem;
			ResourcesAvailable(
				gameSource,
				systemSource,
				shipResource,
				systemResource,
				out resourcesTotal,
				out resourcesFromSystem
			);
		}

		public static void ResourcesAvailable(
			KeyValueListModel gameSource,
			KeyValueListModel systemSource,
			GameKeys.Resource shipResource,
			CelestialSystemKeys.Resource systemResource,
			out float resourcesTotal,
			out float resourcesFromSystem
		)
		{
			if (gameSource == null) throw new ArgumentNullException("gameSource");
			if (systemSource == null) throw new ArgumentNullException("systemSource");
			if (shipResource == null) throw new ArgumentNullException("shipResource");
			if (systemResource == null) throw new ArgumentNullException("systemResource");

			resourcesFromSystem = 0f;

			if (!systemSource.Get(systemResource.Discovered))
			{
				resourcesFromSystem = systemSource.Get(systemResource.GatherMultiplier) * gameSource.Get(shipResource.GatherMultiplier) * gameSource.Get(shipResource.GatherMaximum);
			}

			resourcesTotal = resourcesFromSystem + gameSource.Get(shipResource.Amount);
		}

		public static void ApplyTransit(
			float duration,
			KeyValueListModel gameSource,
			KeyValueListModel systemSource
		)
		{
			if (gameSource == null) throw new ArgumentNullException("gameSource");
			if (systemSource == null) throw new ArgumentNullException("systemSource");

			/*
			// -- To Update

			//float? rations = null;
			//float? rationsRemainingInSystem = null;

			//var propellant = gameSource.Get(KeyDefines.Game.Propellant);
			//var propellantUsage = gameSource.Get(KeyDefines.Game.PropellantUsage) + 1; // Indexing of velocities starts at zero, but we always want to consume full.
			// --

			int rationingMinimum;
			int rationingMaximum;
			int rationingLimit;
			bool rationsInsufficientForLimit;

			if (RationsValidation(
				duration,
				gameSource,
				systemSource,
				out rationingMinimum,
				out rationingMaximum,
				out rationingLimit,
				out rationsInsufficientForLimit
			))
			{
				// Rationing is valid and we can make calculations based on it.

				var rationing = gameSource.Get(KeyDefines.Game.Rationing);
				var population = gameSource.Get(KeyDefines.Game.Population);
				var shipPopulationMaximum = gameSource.Get(KeyDefines.Game.ShipPopulationMaximum);
				var shipPopulationMinimum = gameSource.Get(KeyDefines.Game.ShipPopulationMinimum);
				var populationMinimum = gameSource.Get(KeyDefines.Game.PopulationMinimum);
				var populationMaximum = gameSource.Get(KeyDefines.Game.PopulationMaximumMultiplier) * shipPopulationMaximum;
				var populationRationingMultiplier = gameSource.Get(KeyDefines.Game.PopulationRationingMultiplier);
				var rationsConsumptionMultiplier = gameSource.Get(KeyDefines.Game.RationsConsumptionMultiplier);
				var transitsWithoutRations = gameSource.Get(KeyDefines.Game.TransitsWithoutRations);
				var transitsWithOverPopulation = gameSource.Get(KeyDefines.Game.TransitsWithOverPopulation);
				var transitsWithUnderPopulation = gameSource.Get(KeyDefines.Game.TransitsWithUnderPopulation);

				rationing = Mathf.Min(rationing, rationingLimit);
				var rationsConsumed = RationsConsumed(
					duration,
					population,
					rationsConsumptionMultiplier,
					rationing
				);

				float rationsTotal;
				float rationsFromSystem;
				ResourcesAvailable(
					gameSource,
					systemSource,
					KeyDefines.Game.Rations,
					KeyDefines.CelestialSystem.Rations,
					out rationsTotal,
					out rationsFromSystem
				);

				rationsTotal = Mathf.Max(0f, rationsTotal - rationsConsumed);
				var rationsMaximum = gameSource.Get(KeyDefines.Game.Rations.Maximum);
				var rations = Mathf.Min(rationsMaximum, rationsTotal);
				var rationsRemainingInSystem = Mathf.Min(Mathf.Max(0f, rationsTotal - rations), rationsFromSystem);

				population = Mathf.Clamp(
					population + ((rationsInsufficientForLimit ? rationingMinimum : rationing) * populationRationingMultiplier),
					populationMinimum,
					populationMaximum
				);

				if (rationsInsufficientForLimit) transitsWithoutRations = Mathf.Min(transitsWithoutRations + 1, gameSource.Get(KeyDefines.Game.TransitsWithoutRationsMaximum));
				if (shipPopulationMaximum < population) transitsWithOverPopulation = Mathf.Min(transitsWithOverPopulation + 1, gameSource.Get(KeyDefines.Game.TransitsWithOverPopulationMaximum));
				if (population < shipPopulationMinimum) transitsWithUnderPopulation = Mathf.Min(transitsWithUnderPopulation + 1, gameSource.Get(KeyDefines.Game.TransitsWithUnderPopulationMaximum));

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

				gameSource.Set(
					KeyDefines.Game.Rations.Amount,
					rations
				);

				if (!systemSource.Get(KeyDefines.CelestialSystem.Rations.Discovered))
				{
					systemSource.Set(
						KeyDefines.CelestialSystem.Rations.Discovered,
						true
					);

					systemSource.Set(
						KeyDefines.CelestialSystem.Rations.GatheredAmount,
						rationsFromSystem - rationsRemainingInSystem
					);
				}
				// --
			}
			else
			{
				Debug.LogError(
					"Invalid rationing range [ " + rationingMinimum + " , " + rationingMaximum + " ]" +
	               	"\nThis really shouldn't happen, make sure you initialized the rules properly"
				);
			}

			float propellantTotal;
			float propellantFromSystem;
			ResourcesAvailable(
				gameSource,
				systemSource,
				KeyDefines.Game.Propellant,
				KeyDefines.CelestialSystem.Propellant,
				out propellantTotal,
				out propellantFromSystem
			);

			var propellantUsage = gameSource.Get(KeyDefines.Game.PropellantUsage);

			propellantTotal = Mathf.Max(0f, propellantTotal - propellantUsage);
			var propellantMaximum = gameSource.Get(KeyDefines.Game.Propellant.Maximum);
			var propellant = Mathf.Min(propellantMaximum, propellantTotal);
			var propellantRemainingInSystem = Mathf.Min(Mathf.Max(0f, propellantTotal - propellant), propellantFromSystem);

			propellantUsage = Mathf.Max(1, Mathf.Min(Mathf.FloorToInt(propellant), propellantUsage));

			gameSource.Set(
				KeyDefines.Game.Propellant.Amount,
				Mathf.Floor(propellant)
			);

			gameSource.Set(
				KeyDefines.Game.PropellantUsage,
				propellantUsage
			);

			if (!systemSource.Get(KeyDefines.CelestialSystem.Propellant.Discovered))
			{
				systemSource.Set(
					KeyDefines.CelestialSystem.Propellant.Discovered,
					true
				);

				systemSource.Set(
					KeyDefines.CelestialSystem.Propellant.GatheredAmount,
					propellantRemainingInSystem
				);
			}

			float metallicsTotal;
			float metallicsFromSystem;
			ResourcesAvailable(
				gameSource,
				systemSource,
				KeyDefines.Game.Metallics,
				KeyDefines.CelestialSystem.Metallics,
				out metallicsTotal,
				out metallicsFromSystem
			);

			var metallicsMaximum = gameSource.Get(KeyDefines.Game.Metallics.Maximum);
			var metallics = Mathf.Min(metallicsMaximum, metallicsTotal);
			var metallicsRemainingInSystem = Mathf.Min(Mathf.Max(0f, metallicsTotal - metallics), metallicsFromSystem);

			gameSource.Set(
				KeyDefines.Game.Metallics.Amount,
				metallics
			);

			if (!systemSource.Get(KeyDefines.CelestialSystem.Metallics.Discovered))
			{
				systemSource.Set(
					KeyDefines.CelestialSystem.Metallics.Discovered,
					true
				);

				systemSource.Set(
					KeyDefines.CelestialSystem.Metallics.GatheredAmount,
					metallicsRemainingInSystem
				);
			}
			*/
		}
	}
}