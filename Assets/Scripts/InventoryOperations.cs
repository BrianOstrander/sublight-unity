namespace LunraGames.SpaceFarm
{
	/// <summary>
	/// The types of operations that can be carried out on the inventory.
	/// </summary>
	public enum InventoryOperations
	{
		Unknown = 0,
		/// <summary>
		/// Adds resources to the ship's inventory.
		/// </summary>
		AddResource = 10
		// SubtractResource - Subtracts resources from the ship's inventory.
		// AddInstance - Adds a new instance of an item, probably will require some kind of inventory service with IO.
		// RemoveInstance - Removes an instance of an item.
	}
}