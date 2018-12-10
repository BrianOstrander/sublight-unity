using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ShipModel : Model
	{
		#region Serialized
		[JsonProperty] UniversePosition position;

		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;

		[JsonProperty] TravelRange travelRange;
		[JsonIgnore]
		ListenerProperty<TravelRange> travelRangeListener;
		[JsonIgnore]
		public readonly ReadonlyProperty<TravelRange> TravelRange;

		[JsonProperty] InventoryListModel inventory = new InventoryListModel();
		[JsonIgnore]
		public InventoryListModel Inventory { get { return inventory; } }
		#endregion

		public ShipModel()
		{
			// Assigned Values
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			TravelRange = new ReadonlyProperty<TravelRange>(value => travelRange = value, () => travelRange, out travelRangeListener);
		}

		#region Utility
		public void SetMinimumTravelRange(float minimum) { travelRangeListener.Value = travelRangeListener.Value.NewMinimum(minimum); }
		#endregion
	}
}