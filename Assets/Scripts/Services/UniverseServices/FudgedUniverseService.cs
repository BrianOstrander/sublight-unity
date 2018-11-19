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
			/*
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
			*/

			/*
			var positions = new Vector3[]
			{
				new Vector3(0.10f, 0f, 0.85f),
				new Vector3(0.25f, 0f, 0.20f),
				new Vector3(0.95f, 0f, 0.30f),
				new Vector3(0.10f, 0f, 0.55f),
				new Vector3(0.70f, 0f, 0.60f),


				new Vector3(0.20f, 0f, 0.15f),
				new Vector3(0.15f, 0f, 0.95f),
				new Vector3(0.40f, 0f, 0.05f),

				new Vector3(0.90f, 0f, 0.10f),
				new Vector3(0.90f, 0f, 0.95f),
				new Vector3(0.70f, 0f, 0.45f),
				// None in x 0.45f

				new Vector3(0.95f, 0f, 0.25f),
				new Vector3(0.40f, 0f, 0.80f),
				new Vector3(0.65f, 0f, 0.65f),
				new Vector3(0.30f, 0f, 0.75f),
				new Vector3(0.10f, 0f, 0.35f),
				new Vector3(0.90f, 0f, 0.65f),


				new Vector3(0.25f, 0f, 0.10f),
				new Vector3(0.05f, 0f, 0.05f),
				new Vector3(0.05f, 0f, 0.75f),
				new Vector3(0.85f, 0f, 0.05f),
				new Vector3(0.05f, 0f, 0.30f),
				new Vector3(0.10f, 0f, 0.15f),

				new Vector3(0.60f, 0f, 0.35f),
				new Vector3(0.80f, 0f, 0.10f),
				new Vector3(0.85f, 0f, 0.15f),
				new Vector3(0.20f, 0f, 0.25f),
				new Vector3(0.75f, 0f, 0.05f),


				new Vector3(0.10f, 0f, 0.05f),
				new Vector3(0.25f, 0f, 0.75f),
				new Vector3(0.90f, 0f, 0.55f),

				new Vector3(0.85f, 0f, 0.40f),
				new Vector3(0.10f, 0f, 0.65f),

				new Vector3(0.60f, 0f, 0.20f),
				new Vector3(0.50f, 0f, 0.90f),

				new Vector3(0.95f, 0f, 0.85f),

				new Vector3(0.85f, 0f, 0.80f),
				new Vector3(0.60f, 0f, 0.75f),
				new Vector3(0.30f, 0f, 0.45f),
				new Vector3(0.90f, 0f, 0.20f),
				new Vector3(0.30f, 0f, 0.30f),
				new Vector3(0.70f, 0f, 0.20f),
				new Vector3(0.75f, 0f, 0.55f),
				new Vector3(0.35f, 0f, 0.10f),
				new Vector3(0.35f, 0f, 0.20f),


				new Vector3(0.50f, 0f, 0.35f),
				new Vector3(0.70f, 0f, 0.65f),
				new Vector3(0.65f, 0f, 0.70f),
				new Vector3(0.85f, 0f, 0.50f),
				new Vector3(0.80f, 0f, 0.45f),
				new Vector3(0.40f, 0f, 0.35f),
				new Vector3(0.65f, 0f, 0.15f),
				new Vector3(0.55f, 0f, 0.95f),
				new Vector3(0.90f, 0f, 0.70f),
				new Vector3(0.95f, 0f, 0.35f),
				new Vector3(0.75f, 0f, 0.15f),

				new Vector3(0.25f, 0f, 0.45f),

				new Vector3(0.50f, 0f, 0.20f),
				new Vector3(0.70f, 0f, 0.50f),
				new Vector3(0.55f, 0f, 0.80f),
				new Vector3(0.15f, 0f, 0.45f),
				new Vector3(0.65f, 0f, 0.05f),
				new Vector3(0.15f, 0f, 0.10f),
				new Vector3(0.85f, 0f, 0.75f),
				new Vector3(0.20f, 0f, 0.80f),
				new Vector3(0.15f, 0f, 0.40f),


				new Vector3(0.90f, 0f, 0.60f)
			};
			*/

			var positions = new Vector3[]
			{
				new Vector3(0.1f, 0f, 0.1f),
				new Vector3(0.1f, 0f, 0.9f),
				new Vector3(0.9f, 0f, 0.9f),
				new Vector3(0.9f, 0f, 0.1f),
				new Vector3(0.5f, 0f, 0.5f)
			};

			var demon = new Demon(sector.Seed);
			var seeds = new int[positions.Length];

			for (var i = 0; i < positions.Length; i++)
			{
				seeds[i] = demon.NextInteger;
			}

			var systems = new SystemModel[positions.Length];
			for (var i = 0; i < positions.Length; i++)
			{
				systems[i] = CreateSystem(sector, seeds[i], sector.Position.Value.NewLocal(positions[i]), i);
			}
			sector.SetSystems(systems);
		}

		public override SystemModel CreateSystem(SectorModel sector, int seed, UniversePosition position, int index)
		{
			SystemModel system = new SystemModel();

			system.Index.Value = index;

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

			PopulateSystem(system);

			return system;
		}

		public override void PopulateSystem(SystemModel systemModel)
		{
			var random = new Demon(systemModel.Seed + 1);
			var bodies = new List<BodyModel>();

			var star = new BodyModel();
			bodies.Add(star);
			star.Seed.Value = random.NextInteger;
			star.BodyId.Value = 0;
			star.EncounterWeight.Value = random.NextFloat;
			star.ParentId.Value = -1;
			star.Name.Value = "Primary Star";

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

				switch (random.GetNextInteger(0, 2))
				{
					case 0:
						// star
						var starModel = new BodyModel();
						generic = starModel;
						name = "Companion Star " + Strings.GreekAlpha(companionStar++, true);
						break;
					case 1:
						// terrestrial
						var terrestrialModel = new BodyModel();
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

				bodies.Add(generic);
			}

			systemModel.Bodies.Value = bodies.ToArray();
		}

		#region Utility
		float GetFraction(ref float remaining, float normal)
		{
			return remaining - (remaining = remaining * normal);
		}
		#endregion
	}
}