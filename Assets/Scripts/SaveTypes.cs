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

		// Inventory References
		ModuleReference = 60,
		OrbitalCrewReference = 70,

		LanguageDatabase = 100
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
