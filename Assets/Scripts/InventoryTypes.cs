namespace LunraGames.SpaceFarm
{
	public enum InventoryTypes
	{
		Unknown = 0,
		Resources = 10,
		OrbitalCrew = 20,
		Module = 30
	}

	public static class InventoryValidator
	{
		public static InventoryTypes[] Crews = {
			InventoryTypes.Unknown,
			InventoryTypes.OrbitalCrew
		};
	}
}