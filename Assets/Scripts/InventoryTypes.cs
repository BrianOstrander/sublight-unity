namespace LunraGames.SpaceFarm
{
	public enum InventoryTypes
	{
		Unknown = 0,
		Resources = 10,
		OrbitalProbe = 20,
		OrbitalCrew = 30
	}

	public static class InventoryValidator
	{
		public static InventoryTypes[] Probes = {
			InventoryTypes.Unknown,
			InventoryTypes.OrbitalProbe
		};

		public static InventoryTypes[] Crews = {
			InventoryTypes.Unknown,
			InventoryTypes.OrbitalCrew
		};
	}
}