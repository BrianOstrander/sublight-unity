using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class InventoryFilterEntryModel<T> : ValueFilterEntryModel<T>, IInventoryFilterEntryModel
	{
		[JsonProperty] InventoryFilterTypes inventoryFilterType;

		[JsonIgnore]
		public ListenerProperty<InventoryFilterTypes> InventoryFilterType;

		public InventoryFilterEntryModel()
		{
			InventoryFilterType = new ListenerProperty<InventoryFilterTypes>(value => inventoryFilterType = value, () => inventoryFilterType);
		}

		[JsonIgnore]
		public InventoryFilterTypes FilterInventoryFilterType
		{
			get { return InventoryFilterType.Value; }
			set { InventoryFilterType.Value = value; }
		}
	}

	public interface IInventoryFilterEntryModel : IModel, IValueFilterEntryModel
	{
		/// <summary>
		/// Worst name ever, but does the job.
		/// </summary>
		/// <value>The type of the filter inventory filter.</value>
		InventoryFilterTypes FilterInventoryFilterType { get; set; }
	}
}