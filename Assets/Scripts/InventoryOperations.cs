namespace LunraGames.SubLight
{
	/// <summary>
	/// The types of operations that can be carried out on the inventory.
	/// </summary>
	public enum InventoryOperations
	{
		Unknown = 0,

		/// <summary>
		/// Adds resources to the ship's inventory, negatives are allowed, but
		/// the ship's inventory will never go below zero.
		/// </summary>
		AddResources = 10,
		// MultiplyResources - Multiplies resources in the ships inventory, should have a checkbox for ignoring zero values.
		// ClampResources - Clamps the resources between two values.

		/// <summary>
		/// Adds a new instance of an inventory reference with the specified
		/// InventoryId.
		/// </summary>
		AddInstance = 100
		// RemoveInstance - Removes one, all, or a specified number of items with the specified InventoryId.
		// AddRandomInstance - Adds a new instance of an inventory reference within the specified constraints.
		// RemoveRandomInstance - Removes a random instance of an inventory reference within the specified constraints.
	}
}