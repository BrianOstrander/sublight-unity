namespace LunraGames.SubLight
{
	public enum EncounterLogTypes
	{
		Unknown = 0,
		Text = 10,
		KeyValue = 20,
		Inventory = 30,
		Switch = 40
	}

	public static class EncounterLogValidator
	{
		public static EncounterLogTypes[] Presented = {
			EncounterLogTypes.Unknown,
			EncounterLogTypes.Text
		};

		public static EncounterLogTypes[] Logic = {
			EncounterLogTypes.Unknown,
			EncounterLogTypes.KeyValue,
			EncounterLogTypes.Inventory,
			EncounterLogTypes.Switch
		};
	}
}