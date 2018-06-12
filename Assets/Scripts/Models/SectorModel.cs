using UnityEngine;

namespace LunraGames.SpaceFarm.Models
{
	public class SectorModel : Model
	{
		#region Assigned
		public readonly ModelProperty<UniverseModel> Universe = new ModelProperty<UniverseModel>();
		public readonly ModelProperty<int> Seed = new ModelProperty<int>();
		public readonly ModelProperty<bool> Visited = new ModelProperty<bool>();
		public readonly ModelProperty<UniversePosition> Position = new ModelProperty<UniversePosition>();
		#endregion

		#region Derived
		public readonly ModelProperty<SystemModel[]> Systems = new ModelProperty<SystemModel[]>();
  		#endregion
	}
}