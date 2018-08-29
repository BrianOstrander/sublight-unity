namespace LunraGames.SubLight
{
	/// <summary>
	/// The type of filtering done by an inventory filter.
	/// </summary>
	public enum InventoryFilterTypes
	{
		Unknown = 0,
		/// <summary>
		/// The inventory of the ship.
		/// </summary>
		Inventory = 10,
		/// <summary>
		/// The references for inventory items loaded at runtime.
		/// </summary>
		References = 20
	}
}