﻿using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class UniverseModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] SectorModel[] sectors = new SectorModel[0];

		#region Assigned
		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		#endregion

		#region Derived
		[JsonIgnore]
		public readonly ListenerProperty<SectorModel[]> Sectors;
		#endregion

		public UniverseModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Sectors = new ListenerProperty<SectorModel[]>(value => sectors = value, () => sectors);
		}

		#region Utility
		public SectorModel GetSector(UniversePosition position)
		{
			// TODO: Generate sector and systems if not populated.
			var sector = Sectors.Value.FirstOrDefault(s => s.Position.Value.SectorEquals(position));
			if (sector != null) return sector;

			sector = App.UniverseService.CreateSector(this, position);
			var list = Sectors.Value.ToList();
			list.Add(sector);
			Sectors.Value = list.ToArray();
			return sector;
		}

		public SystemModel GetSystem(UniversePosition position)
		{
			return GetSector(position).GetSystem(position);
		}
  		#endregion
	}
}