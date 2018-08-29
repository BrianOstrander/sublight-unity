namespace LunraGames.SubLight.Models
{
	public class IdInventoryFilterEntryModel : InventoryFilterEntryModel<string>
	{
		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.InventoryId; } }
	}
}