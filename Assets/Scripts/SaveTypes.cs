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

		GalaxyPreview = 60,
		GalaxyDistant = 62,
		GalaxyInfo = 64,

		// Inventory References
		InventoryModule = 200
	}
}
