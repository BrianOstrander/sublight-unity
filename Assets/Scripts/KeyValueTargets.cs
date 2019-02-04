namespace LunraGames.SubLight
{
	public enum KeyValueTargets
	{
		Unknown = 0,
		/// <summary>
		/// Values specific to the currently active encounter.
		/// </summary>
		Encounter = 10,
		/// <summary>
		/// Values specific to the currently active game.
		/// </summary>
		Game = 20,
		/// <summary>
		/// Values shared accross all games.
		/// </summary>
		Global = 30,
		/// <summary>
		/// Values specific to the current system's preferences.
		/// </summary>
		/// <remarks>
		/// This should be used for global, but not strictly gameplay related
		/// values.
		/// related.
		/// </remarks>
		Preferences = 40
	}
}