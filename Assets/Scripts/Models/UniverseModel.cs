using System.Linq;

using UnityEngine;

namespace LunraGames.SpaceFarm.Models
{
	public class UniverseModel : Model
	{
		#region Assigned
		public readonly ModelProperty<int> Seed = new ModelProperty<int>();
		#endregion

		#region Derived
		public readonly ModelProperty<SectorModel[]> Sectors = new ModelProperty<SectorModel[]>();
		#endregion

		#region Utility
		public SectorModel GetSector(UniversePosition position)
		{
			return Sectors.Value.FirstOrDefault(s => s.Position.Value == position);
		}
  		#endregion
	}
}