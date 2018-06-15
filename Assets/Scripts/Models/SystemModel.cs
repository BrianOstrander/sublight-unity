namespace LunraGames.SpaceFarm.Models
{
	public class SystemModel : Model
	{
		#region Assigned
		public virtual SystemTypes SystemType { get { return SystemTypes.Unknown; } }
		public readonly ModelProperty<SectorModel> Sector = new ModelProperty<SectorModel>();
		public readonly ModelProperty<int> Seed = new ModelProperty<int>();
		public readonly ModelProperty<bool> Visited = new ModelProperty<bool>();
		public readonly ModelProperty<UniversePosition> Position = new ModelProperty<UniversePosition>();
		#endregion

		#region Derived
		public readonly ModelProperty<string> Name = new ModelProperty<string>();
		public readonly ModelProperty<float> Rations = new ModelProperty<float>();
  		#endregion
	}
}