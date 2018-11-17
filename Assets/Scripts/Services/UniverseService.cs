﻿using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	public abstract class UniverseService : IUniverseService
	{
		// TODO: Populate more than just the initial sector;
		readonly static UniversePosition[] InitialSectors = {
			UniversePosition.Zero
		};

		public UniverseModel CreateUniverse(CreateGameBlock info)
		{
			var universeModel = new UniverseModel();
			universeModel.Seed.Value = info.GalaxySeed;

			// I don't think I need this...
			PopulateUniverse(universeModel, InitialSectors);

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
			sector.Position.Value = position;
			sector.Visited.Value = false;
			sector.SetSystems(new SystemModel[0]);
			sector.Seed.Value = DemonUtility.CantorPairs(Mathf.FloorToInt(sector.Position.Value.Sector.x), Mathf.FloorToInt(sector.Position.Value.Sector.z), universe.Seed);
			PopulateSector(sector);
			return sector;
		}

		public abstract void PopulateSector(SectorModel sector);
		public abstract SystemModel CreateSystem(SectorModel sector, int seed, UniversePosition position, int index);
		public abstract void PopulateSystem(SystemModel systemModel);
	}

	public interface IUniverseService
	{
		/// <summary>
		/// Creates a universe model with initial sectors and systems populated.
		/// </summary>
		/// <returns>The universe.</returns>
		/// <param name="info">Info.</param>
		UniverseModel CreateUniverse(CreateGameBlock info);
		/// <summary>
		/// Populates the specified sectors of the universe with systems.
		/// </summary>
		/// <remarks>
		/// If any of the supplied sectors have already been visited, they are
		/// not populated again.
		/// </remarks>
		/// <param name="universe">Universe.</param>
		/// <param name="sectorPositions">Sector positions.</param>
		void PopulateUniverse(UniverseModel universe, params UniversePosition[] sectorPositions);
		/// <summary>
		/// Creates a sector at the specified position and populates it with
		/// systems.
		/// </summary>
		/// <returns>The sector.</returns>
		/// <param name="universe">Universe.</param>
		/// <param name="position">Position.</param>
		SectorModel CreateSector(UniverseModel universe, UniversePosition position);
		/// <summary>
		/// Populates the specified sector with systems.
		/// </summary>
		/// <param name="sector">Sector.</param>
		void PopulateSector(SectorModel sector);
		/// <summary>
		/// Creates a system in the specified sector.
		/// </summary>
		/// <returns>The system.</returns>
		/// <param name="systemType">System type.</param>
		/// <param name="sector">Sector.</param>
		/// <param name="seed">Seed.</param>
		/// <param name="position">Position.</param>
		SystemModel CreateSystem(SectorModel sector, int seed, UniversePosition position, int index);
		/// <summary>
		/// Populates a star system.
		/// </summary>
		/// <param name="celestialModel">Star model.</param>
		void PopulateSystem(SystemModel systemModel);
	}
}