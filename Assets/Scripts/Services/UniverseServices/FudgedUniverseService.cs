using System;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
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
				types[i] = SystemTypes.Star;
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
				case SystemTypes.Star: system = new StarModel(); break;
				default: throw new ArgumentException("Unsupported SystemType " + systemType, "systemType");
			}

			system.Seed.Value = seed;
			system.Visited.Value = false;
			system.Position.Value = position;

			system.Name.Value = seed.ToString();
			system.Rations.Value = 0.2f;

			switch (systemType)
			{
				case SystemTypes.Star: PopulateSystem(system as StarModel); break;
				default: throw new ArgumentException("Unsupported SystemType " + systemType, "systemType");
			}

			return system;
		}

		public override void PopulateSystem(StarModel starModel)
		{
			// TODO: Populate star specific info
		}
	}
}