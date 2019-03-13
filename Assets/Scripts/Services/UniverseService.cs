using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using LunraGames.NumberDemon;
using LunraGames.SubLight.Models;

namespace LunraGames.SubLight
{
	/*
	public struct SectorRequest
	{
		public static SectorRequest Failure(string error)
		{
			return new SectorRequest
			{
				Status = RequestStatus.Failure,
				Error = error
			};
		}

		public static SectorRequest Success(SectorModel sector)
		{
			return new SectorRequest
			{
				Status = RequestStatus.Success,
				Sector = sector
			};
		}

		public RequestStatus Status;
		public string Error;
		public SectorModel Sector;
	}

	public struct SystemRequest
	{
		public static SystemRequest Failure(string error)
		{
			return new SystemRequest
			{
				Status = RequestStatus.Failure,
				Error = error
			};
		}

		public static SystemRequest Success(SectorModel sector, SystemModel system)
		{
			return new SystemRequest
			{
				Status = RequestStatus.Success,
				Sector = sector,
				System = system
			};
		}

		public RequestStatus Status;
		public string Error;
		public SectorModel Sector;
		public SystemModel System;
	}

	public struct BodyRequest
	{
		public static BodyRequest Failure(string error)
		{
			return new BodyRequest
			{
				Status = RequestStatus.Failure,
				Error = error
			};
		}

		public static BodyRequest Success(SectorModel sector, SystemModel system, BodyModel body)
		{
			return new BodyRequest
			{
				Status = RequestStatus.Success,
				Sector = sector,
				System = system,
				Body = body
			};
		}

		public RequestStatus Status;
		public string Error;
		public SectorModel Sector;
		public SystemModel System;
		public BodyModel Body;
	}
	*/

	public abstract class UniverseService : IUniverseService
	{
		public UniverseModel CreateUniverse(CreateGameBlock info)
		{
			var universeModel = new UniverseModel();

			universeModel.Seed.Value = info.GalaxySeed;

			return universeModel;
		}

		public SectorModel GetSector(
			GalaxyInfoModel galaxy,
			UniverseModel universe,
			UniversePosition sectorPosition
		)
		{
			var sector = universe.Sectors.Value.FirstOrDefault(s => s.Position.Value.SectorEquals(sectorPosition));
			return GetGeneratedSector(galaxy, universe, sector, sectorPosition);
		}

		public SystemModel GetSystem(
			GalaxyInfoModel galaxy,
			UniverseModel universe,
			SectorModel sector,
			int systemIndex
		)
		{
			var system = sector.Systems.Value.FirstOrDefault(s => s.Index.Value == systemIndex);
			return GetGeneratedSystem(galaxy, universe, sector, system, systemIndex);
		}

		public SystemModel GetSystem(
			GalaxyInfoModel galaxy,
			UniverseModel universe,
			UniversePosition sectorPosition,
			int systemIndex
		)
		{
			return GetSystem(galaxy, universe, GetSector(galaxy, universe, sectorPosition), systemIndex);
		}

		SectorModel GetGeneratedSector(
			GalaxyInfoModel galaxy,
			UniverseModel universe,
			SectorModel sector,
			UniversePosition sectorPosition
		)
		{
			if (sector == null)
			{
				sector = OnCreateSector(CreateSector(universe, sectorPosition));
				universe.Sectors.Value = universe.Sectors.Value.Append(sector).ToArray();
			}
			else if (sector.IsGenerated)
			{
				return sector;
			}

			var generated = new List<SystemModel>();
			var ignoredIndices = new List<int>();

			if (sector.Visited)
			{
				foreach (var system in sector.Systems.Value)
				{
					generated.Add(system);
					ignoredIndices.Add(system.Index.Value);
				}
			}

			var bodyMapPixel = galaxy.GetBodyMapPixel(sectorPosition);
			sector.SystemCount.Value = galaxy.SectorBodyCount(bodyMapPixel.RgbAverage);
			sector.Systems.Value = new SystemModel[sector.SystemCount.Value];

			for (var i = 0; i < sector.SystemCount.Value; i++)
			{
				if (ignoredIndices.Contains(i))
				{
					ignoredIndices.Remove(i);
					continue;
				}
				generated.Add(OnCreateSystem(CreateSystem(sector, i)));
			}

			sector.IsGenerated = true;
			sector.Systems.Value = generated.ToArray();

			return sector;
		}

		SystemModel GetGeneratedSystem(
			GalaxyInfoModel galaxy,
			UniverseModel universe,
			SectorModel sector,
			SystemModel system,
			int systemIndex
		)
		{
			if (system == null)
			{
				system = OnCreateSystem(CreateSystem(sector, systemIndex));
				sector.Systems.Value = sector.Systems.Value.Append(system).ToArray();
			}
			else if (system.IsGenerated)
			{
				return system;
			}

			// TODO: Body specific logic here...

			system.IsGenerated = true;

			return system;
		}

		SectorModel CreateSector(
			UniverseModel universe,
			UniversePosition sectorPosition
		)
		{
			var sector = new SectorModel();
			sector.Position.Value = sectorPosition;
			sector.Visited.Value = false;
			sector.Systems.Value = new SystemModel[0];
			sector.Seed.Value = DemonUtility.CantorPairs(Mathf.FloorToInt(sector.Position.Value.Sector.x), Mathf.FloorToInt(sector.Position.Value.Sector.z), universe.Seed);
			return sector;
		}

		SystemModel CreateSystem(
			SectorModel sector,
			int index
		)
		{
			SystemModel system = new SystemModel();
			system.Index.Value = index;
			system.Seed.Value = DemonUtility.CantorPairs(index, sector.Seed);
			system.Visited.Value = false;
			system.Position.Value = GetPositionInSector(
				sector.Position.Value,
				SystemModel.Seeds.Position(system.Seed.Value),
				index,
				sector.SystemCount.Value
			);

			var seedString = system.Seed.Value.ToString();
			if (4 < seedString.Length) seedString = seedString.Substring(0, 4);

			system.Name.Value = "Star System - " + seedString;
			return system;
		}

		protected abstract UniversePosition GetPositionInSector(
			UniversePosition sectorPosition,
			int seed,
			int index,
			int systemCount
		);

		protected virtual SectorModel OnCreateSector(SectorModel sector) { return sector; }
		protected virtual SystemModel OnCreateSystem(SystemModel systemModel) { return systemModel; }
	}

	public interface IUniverseService
	{
		/// <summary>
		/// Creates a universe model with initial sectors and systems populated.
		/// </summary>
		/// <returns>The universe.</returns>
		/// <param name="info">Info.</param>
		UniverseModel CreateUniverse(CreateGameBlock info);

		SectorModel GetSector(
			GalaxyInfoModel galaxy,
			UniverseModel universe,
			UniversePosition sectorPosition
		);

		SystemModel GetSystem(
			GalaxyInfoModel galaxy,
			UniverseModel universe,
			SectorModel sector,
			int systemIndex
		);

		SystemModel GetSystem(
			GalaxyInfoModel galaxy,
			UniverseModel universe,
			UniversePosition sectorPosition,
			int systemIndex
		);
	}
}