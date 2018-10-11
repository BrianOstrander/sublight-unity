namespace LunraGames.SubLight
{
	public enum SaveTypes
	{
		Unknown = 0,

		Game = 10,
		Preferences = 20,

		EncounterInfo = 30,

		// Interactions
		InteractedEncounterInfoList = 40,
		InteractedInventoryReferenceList = 45,

		GlobalKeyValues = 50,

		GalaxyInfo = 60,

		// Inventory References
		ModuleReference = 200,
		OrbitalCrewReference = 210
	}

	public static class SaveTypeValidator
	{
		public static SaveTypes[] InventoryReferences = {
			SaveTypes.Unknown,
			SaveTypes.ModuleReference,
			SaveTypes.OrbitalCrewReference
		};

	}
}
