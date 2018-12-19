using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ShipModel : Model
	{
		#region Serialized
		[JsonProperty] UniversePosition position;
		[JsonIgnore] public readonly ListenerProperty<UniversePosition> Position;

		[JsonProperty] TransitRange range = TransitRange.Default;
		[JsonIgnore] ListenerProperty<TransitRange> rangeListener;
		[JsonIgnore] public readonly ReadonlyProperty<TransitRange> Range;

		[JsonProperty] TransitVelocity velocity = TransitVelocity.Default;
		[JsonIgnore] ListenerProperty<TransitVelocity> velocityListener;
		[JsonIgnore] public readonly ReadonlyProperty<TransitVelocity> Velocity;

		[JsonProperty] InventoryListModel inventory = new InventoryListModel();
		[JsonIgnore] public InventoryListModel Inventory { get { return inventory; } }
		#endregion

		public ShipModel()
		{
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Range = new ReadonlyProperty<TransitRange>(value => range = value, () => range, out rangeListener);
			Velocity = new ReadonlyProperty<TransitVelocity>(value => velocity = value, () => velocity, out velocityListener);
		}

		#region Utility
		public void SetRangeMinimum(float minimum) { rangeListener.Value = rangeListener.Value.NewMinimum(minimum); }

		public void SetVelocityMinimum(float minimum) { velocityListener.Value = velocityListener.Value.NewVelocityMinimum(minimum); }
		public void SetVelocityMultiplierCurrent(int multiplier) { velocityListener.Value = velocityListener.Value.NewMultiplierCurrent(multiplier); }
		public void SetVelocityMultiplierMaximum(int maximum) { velocityListener.Value = velocityListener.Value.NewMultiplierMaximum(maximum); }
		public void SetVelocityMultiplierEnabledMaximum(int enabledMaximum) { velocityListener.Value = velocityListener.Value.NewMultiplierEnabledMaximum(enabledMaximum); }
		#endregion
	}
}