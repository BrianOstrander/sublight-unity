namespace LunraGames.SubLight.Models
{
	public class TagInventoryFilterEntryModel : InventoryFilterEntryModel<string>
	{
		public override ValueFilterTypes FilterType { get { return ValueFilterTypes.InventoryTag; } }
	}
}