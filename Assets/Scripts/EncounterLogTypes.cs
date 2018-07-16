namespace LunraGames.SpaceFarm
{
	public enum EncounterLogTypes
	{
		Unknown = 0,
		Text = 10,
		KeyValue = 20
	}

	public static class EncounterLogValidator
	{
		public static EncounterLogTypes[] Presented = {
			EncounterLogTypes.Unknown,
			EncounterLogTypes.Text
		};

		public static EncounterLogTypes[] Logic = {
			EncounterLogTypes.Unknown,
			EncounterLogTypes.KeyValue
		};
	}
}