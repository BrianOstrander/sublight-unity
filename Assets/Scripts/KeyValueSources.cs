namespace LunraGames.SubLight
{
	public enum KeyValueSources
	{
		Unknown = 0,
		/// <summary>
		/// The value is specified already.
		/// </summary>
		Value = 10,
		/// <summary>
		/// Another keyvalue needs to be queried to do the comparison.
		/// </summary>
		KeyValue = 20
	}
}