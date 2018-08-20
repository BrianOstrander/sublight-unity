using System;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public class FudgedUniverseService : UniverseService
	{
		public override void PopulateSector(SectorModel sector)
		{
			var positions = new Vector3[]
			{
				new Vector3(0.05f, 0f, 0.75f),
				new Vector3(0.05f, 0f, 0.30f),
				new Vector3(0.05f, 0f, 0.05f),

				new Vector3(0.10f, 0f, 0.85f),
				new Vector3(0.10f, 0f, 0.65f),
				new Vector3(0.10f, 0f, 0.55f),
				new Vector3(0.10f, 0f, 0.35f),
				new Vector3(0.10f, 0f, 0.15f),
				new Vector3(0.10f, 0f, 0.05f),

				new Vector3(0.15f, 0f, 0.95f),
				new Vector3(0.15f, 0f, 0.45f),
				new Vector3(0.15f, 0f, 0.40f),
				new Vector3(0.15f, 0f, 0.10f),

				new Vector3(0.20f, 0f, 0.80f),
				new Vector3(0.20f, 0f, 0.25f),
				new Vector3(0.20f, 0f, 0.15f),

				new Vector3(0.25f, 0f, 0.75f),
				new Vector3(0.25f, 0f, 0.45f),
				new Vector3(0.25f, 0f, 0.20f),
				new Vector3(0.25f, 0f, 0.10f),

				new Vector3(0.30f, 0f, 0.75f),
				new Vector3(0.30f, 0f, 0.45f),
				new Vector3(0.30f, 0f, 0.30f),

				new Vector3(0.35f, 0f, 0.20f),
				new Vector3(0.35f, 0f, 0.10f),

				new Vector3(0.40f, 0f, 0.80f),
				new Vector3(0.40f, 0f, 0.35f),
				new Vector3(0.40f, 0f, 0.05f),

				// None in x 0.45f

				new Vector3(0.50f, 0f, 0.90f),
				new Vector3(0.50f, 0f, 0.35f),
				new Vector3(0.50f, 0f, 0.20f),

				new Vector3(0.55f, 0f, 0.95f),
				new Vector3(0.55f, 0f, 0.80f),

				new Vector3(0.60f, 0f, 0.75f),
				new Vector3(0.60f, 0f, 0.35f),
				new Vector3(0.60f, 0f, 0.20f),

				new Vector3(0.65f, 0f, 0.70f),
				new Vector3(0.65f, 0f, 0.65f),
				new Vector3(0.65f, 0f, 0.15f),
				new Vector3(0.65f, 0f, 0.05f),

				new Vector3(0.70f, 0f, 0.65f),
				new Vector3(0.70f, 0f, 0.60f),
				new Vector3(0.70f, 0f, 0.50f),
				new Vector3(0.70f, 0f, 0.45f),
				new Vector3(0.70f, 0f, 0.20f),

				new Vector3(0.75f, 0f, 0.55f),
				new Vector3(0.75f, 0f, 0.15f),
				new Vector3(0.75f, 0f, 0.05f),

				new Vector3(0.80f, 0f, 0.45f),
				new Vector3(0.80f, 0f, 0.10f),

				new Vector3(0.85f, 0f, 0.80f),
				new Vector3(0.85f, 0f, 0.75f),
				new Vector3(0.85f, 0f, 0.50f),
				new Vector3(0.85f, 0f, 0.40f),
				new Vector3(0.85f, 0f, 0.15f),
				new Vector3(0.85f, 0f, 0.05f),

				new Vector3(0.90f, 0f, 0.95f),
				new Vector3(0.90f, 0f, 0.70f),
				new Vector3(0.90f, 0f, 0.65f),
				new Vector3(0.90f, 0f, 0.60f),
				new Vector3(0.90f, 0f, 0.55f),
				new Vector3(0.90f, 0f, 0.20f),
				new Vector3(0.90f, 0f, 0.10f),

				new Vector3(0.95f, 0f, 0.85f),
				new Vector3(0.95f, 0f, 0.35f),
				new Vector3(0.95f, 0f, 0.30f),
				new Vector3(0.95f, 0f, 0.25f)
			};

			var demon = new Demon(sector.Seed);
			var types = new SystemTypes[positions.Length];
			var seeds = new int[positions.Length];

			for (var i = 0; i < positions.Length; i++)
			{
				types[i] = SystemTypes.Celestial;
				seeds[i] = demon.NextInteger;
			}

			var systems = new SystemModel[positions.Length];
			for (var i = 0; i < positions.Length; i++)
			{
				systems[i] = CreateSystem(types[i], sector, seeds[i], sector.Position.Value.NewSystem(positions[i]));
			}
			sector.Systems.Value = systems;
		}

		public override SystemModel CreateSystem(SystemTypes systemType, SectorModel sector, int seed, UniversePosition position)
		{
			SystemModel system;

			switch (systemType)
			{
				case SystemTypes.Celestial: system = new CelestialSystemModel(); break;
				default: throw new ArgumentException("Unsupported SystemType " + systemType, "systemType");
			}

			var random = new Demon(seed);

			system.Seed.Value = seed;
			system.Visited.Value = false;
			system.Position.Value = position;

			system.Name.Value = "Star System - " + seed.ToString().Substring(0, 4);

			/*
			switch (random.GetNextInteger(max: 3))
			{
				case 0:
					// Just rations
					system.Rations.Value = random.GetNextFloat(0.1f, 0.75f);
					break;
				case 1:
					// Just fuel
					system.Fuel.Value = random.GetNextInteger(2, 5);
					break;
				case 2:
					// Fuel and Rations
					system.Rations.Value = random.GetNextFloat(0.1f, 0.75f);
					system.Fuel.Value = random.GetNextInteger(2, 5);
					break;
			}
			*/

			system.RationsDetection.Value = random.NextFloat;
			system.FuelDetection.Value = random.NextFloat;

			switch (systemType)
			{
				case SystemTypes.Celestial: PopulateSystem(system as CelestialSystemModel); break;
				default: throw new ArgumentException("Unsupported SystemType " + systemType, "systemType");
			}

			return system;
		}

		public override void PopulateSystem(CelestialSystemModel celestialModel)
		{
			var random = new Demon(celestialModel.Seed + 1);
			var bodies = new List<BodyModel>();

			var rationsRemaining = celestialModel.Rations.Value;
			var fuelRemaining = celestialModel.Fuel.Value;

			var star = new StarBodyModel();
			bodies.Add(star);
			star.Seed.Value = random.NextInteger;
			star.BodyId.Value = 0;
			star.EncounterWeight.Value = random.NextFloat;
			star.ParentId.Value = -1;
			star.Name.Value = "Primary Star";
			star.Resources.Rations.Value = GetFraction(ref rationsRemaining, random.NextFloat);
			star.Resources.Fuel.Value = GetFraction(ref fuelRemaining, random.NextFloat);

			var bodyCount = 0;

			var companionStar = 0;
			var terrestrial = 0;

			for (var i = 0; i < random.GetNextInteger(0, 3); i++)
			{
				BodyModel generic;
				var seed = random.NextInteger;
				var bodyId = (bodyCount += 1);
				var encounterWeight = random.NextFloat;
				var parentId = -1;
				var name = string.Empty;
				var rations = GetFraction(ref rationsRemaining, random.NextFloat);
				var fuel = GetFraction(ref fuelRemaining, random.NextFloat);

				switch (random.GetNextInteger(0, 2))
				{
					case 0:
						// star
						var starModel = new StarBodyModel();
						generic = starModel;
						name = "Companion Star " + Strings.GreekAlpha(companionStar++, true);
						break;
					case 1:
						// terrestrial
						var terrestrialModel = new TerrestrialBodyModel();
						terrestrialModel.ParentId.Value = star.BodyId;
						generic = terrestrialModel;

						name = "Terrestrial " + Strings.GreekAlpha(terrestrial++, true);
						break;
					default: throw new ArgumentException("Random body out of range.");
				}

				generic.Seed.Value = seed;
				generic.BodyId.Value = bodyId;
				generic.EncounterWeight.Value = encounterWeight;
				generic.ParentId.Value = parentId;
				generic.Name.Value = name;
				generic.Resources.Rations.Value = rations;
				generic.Resources.Fuel.Value = fuel;

				bodies.Add(generic);
			}

			star.Resources.Rations.Value += rationsRemaining;
			star.Resources.Fuel.Value += fuelRemaining;

			celestialModel.Bodies.Value = bodies.ToArray();
		}

		#region Utility
		float GetFraction(ref float remaining, float normal)
		{
			return remaining - (remaining = remaining * normal);
		}
		#endregion
	}
}