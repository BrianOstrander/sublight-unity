using System.Linq;

using Newtonsoft.Json;

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

		#region Utility
		public SectorModel GetSector(UniversePosition position)
		{
			// TODO: Generate sector and systems if not populated.
			var sector = Sectors.Value.FirstOrDefault(s => s.Position.Value.SectorEquals(position));
			if (sector != null) return sector;

			sector = App.Universe.CreateSector(this, position);
			var list = Sectors.Value.ToList();
			list.Add(sector);
			Sectors.Value = list.ToArray();
			return sector;
		}

		public SystemModel GetSystem(UniversePosition position)
		{
			return GetSector(position).GetSystem(position);
		}

		public BodyModel GetBody(UniversePosition position, int id)
		{
			return GetSystem(position).GetBody(id);
		}
  		#endregion
	}
}