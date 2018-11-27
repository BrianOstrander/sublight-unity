﻿using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class UniverseModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] SectorModel[] sectors = new SectorModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<SectorModel[]> Sectors;

		public UniverseModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Sectors = new ListenerProperty<SectorModel[]>(value => sectors = value, () => sectors);
		}
	}
}