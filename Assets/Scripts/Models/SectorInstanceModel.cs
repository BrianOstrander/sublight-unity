using System;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SectorInstanceModel : Model
	{
		int maxXZOffset;
		int totalOffset;

		int currX;
		int currZ;

		UniverseModel universe;

		[JsonIgnore]
		public UniversePosition Sector { get; private set; }
		[JsonIgnore]
		public SystemInstanceModel[] SystemModels { get; private set; }

		void GetXZ(UniversePosition position, out int x, out int z)
		{
			x = (int)position.Sector.x;
			z = (int)position.Sector.z;
		}

		public SectorInstanceModel(
			UniverseModel universe,
			UniversePosition sector,
			int maxXZOffset,
			int maxSystems
		)
		{
			this.universe = universe;
			Sector = sector;
			GetXZ(sector, out currX, out currZ);
			this.maxXZOffset = maxXZOffset;
			totalOffset = (maxXZOffset * 2) + 1;

			SystemModels = new SystemInstanceModel[maxSystems];
			for (var i = 0; i < maxSystems; i++)
			{
				var curr = new SystemInstanceModel(universe, i);
				curr.SetSector(sector);
				SystemModels[i] = curr;
			}
		}

		public void SetSectorOrigin(UniversePosition origin)
		{
			//var originX = 0;
			//var originZ = 0;
			//SetXZ(sectorOrigin, out originX, out originZ);

			//var deltaX = currX - originX;
			//var deltayZ = currZ - originZ;

			//var xInBounds = Mathf.Abs(deltaX) <= maxXZOffset;
			//var zInBounds = Mathf.Abs(deltayZ) <= maxXZOffset;

			//if (xInBounds && zInBounds) return;

			//if (!xInBounds)
			//{
			//	var xBegin = originX - maxXZOffset;

			//	var xDirection = Math.Sign(deltaX);

			//	var howOutOfBounds = (deltaX - (maxXZOffset * xDirection)) % totalOffset;

			//	var newX = xBegin + totalOffset + howOutOfBounds;
			//}

			var originX = 0;
			var originZ = 0;
			GetXZ(origin, out originX, out originZ);

			var newX = 0;
			var newZ = 0;

			var xInBounds = CheckBounds(originX, currX, out newX);
			var zInBounds = CheckBounds(originZ, currZ, out newZ);

			if (xInBounds & zInBounds) return;

			Sector = new UniversePosition(new Vector3(newX, 0f, newZ), Vector3.zero);

			foreach (var system in SystemModels) system.SetSector(Sector);
		}

		bool CheckBounds(int origin, int current, out int newValue)
		{
			newValue = current;

			var delta = current - origin;

			if (Mathf.Abs(delta) <= maxXZOffset) return true;

			var begin = origin - maxXZOffset;
			var direction = Math.Sign(delta);
			var howOutOfBounds = (delta - (maxXZOffset * direction)) % totalOffset;
			newValue = begin + totalOffset + howOutOfBounds;
			return false;
		}
	}
}