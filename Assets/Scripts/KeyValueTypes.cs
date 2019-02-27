namespace LunraGames.SubLight
{
	public enum KeyValueTypes
	{
		Unknown = 0,

		Boolean = 10,
		Integer = 20,
		String = 30,
		Float = 40
	}

	/// <summary>
	/// Enums are stored as integers, but not in the same place, so... I dunno...
	/// </summary>
	public enum KeyValueExtendedTypes
	{
		Unknown = 0,

		Boolean = 10,
		Integer = 20,
		String = 30,
		Float = 40,
		Enum = 50
	}
}