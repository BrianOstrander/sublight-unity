using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SpaceFarm.Models;

namespace LunraGames.SpaceFarm
{
	public class UniverseService
	{
		public UniverseModel CreateUniverse(int seed)
		{
			var universeModel = new UniverseModel();
			universeModel.Seed.Value = seed;
			universeModel.Sectors.Value = new SectorModel[0];
			PopulateUniverse(universeModel, UniversePosition.Zero);
			return universeModel;
		}

		public void PopulateUniverse(UniverseModel universe, params UniversePosition[] sectorPositions)
		{
			var newSectors = new List<SectorModel>(universe.Sectors.Value);
			foreach (var sectorPosition in sectorPositions)
			{
				if (universe.Sectors.Value.FirstOrDefault(s => s.Position.Value == sectorPosition) != null) continue;
				newSectors.Add(CreateSector(universe, sectorPosition));
			}
			universe.Sectors.Value = newSectors.ToArray();
		}

		public SectorModel CreateSector(UniverseModel universe, UniversePosition position)
		{
			var sector = new SectorModel();
			sector.Universe.Value = universe;
			sector.Position.Value = position;
			sector.Visited.Value = false;
			sector.Systems.Value = new SystemModel[0];
			sector.Seed.Value = DemonUtility.CantorPairs(Mathf.FloorToInt(sector.Position.Value.Sector.x), Mathf.FloorToInt(sector.Position.Value.Sector.z), universe.Seed);
			PopulateSector(sector);
			return sector;
		}

		public void PopulateSector(SectorModel sector)
		{
			// TODO: Randomize all these...

			var positions = new Vector3[]
			{
				new Vector3(0.01f, 0f, 0.01f),
				new Vector3(0.02f, 0f, 0.09f),
				new Vector3(0.03f, 0f, 0.07f),
				new Vector3(0.02f, 0f, 0.04f),
				new Vector3(0.05f, 0f, 0.05f),
				new Vector3(0.06f, 0f, 0.06f)
			};

			var types = new SystemTypes[]
			{
				SystemTypes.Star,
				SystemTypes.Star,
				SystemTypes.Star,
				SystemTypes.Star,
				SystemTypes.Star,
				SystemTypes.Star
			};

			var seeds = new int[]
			{
				100,
				101,
				102,
				103,
				104,
				105
			};

			var systems = new SystemModel[positions.Length];
			for (var i = 0; i < positions.Length; i++)
			{
				systems[i] = CreateSystem(types[i], sector, seeds[i], new UniversePosition(Vector3.zero, positions[i]));
			}
			sector.Systems.Value = systems;
		}

		public SystemModel CreateSystem(SystemTypes systemType, SectorModel sector, int seed, UniversePosition position)
		{
			SystemModel system;

			switch(systemType)
			{
				case SystemTypes.Star: system = new StarModel(); break;
				default: throw new ArgumentException("Unsupported SystemType " + systemType, "systemType");
			}

			system.Sector.Value = sector;
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

		public void PopulateSystem(StarModel starModel)
		{
			// TODO: Populate star specific info
		}
	}
}