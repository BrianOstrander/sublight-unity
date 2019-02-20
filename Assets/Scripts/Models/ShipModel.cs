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

		[JsonProperty] int systemIndex;
		/// <summary>
		/// The index of the current system.
		/// </summary>
		/// <remarks>
		/// This should not be set manually outside of
		/// GameModel.SetCurrentSystem or GameService.
		/// </remarks>
		[JsonIgnore] public readonly ListenerProperty<int> SystemIndex;
		#endregion

		#region NonSerialized

		#endregion

		public ShipModel()
		{
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Range = new ReadonlyProperty<TransitRange>(value => range = value, () => range, out rangeListener);
			Velocity = new ReadonlyProperty<TransitVelocity>(value => velocity = value, () => velocity, out velocityListener);
			SystemIndex = new ListenerProperty<int>(value => systemIndex = value, () => systemIndex);
		}

		#region Utility
		public void SetRangeMinimum(float minimum) { rangeListener.Value = rangeListener.Value.NewMinimum(minimum); }

		// TODO: Somehow initialize these values in the rules initializition and listen for changes...

		public void SetVelocityMinimum(float minimum) { velocityListener.Value = velocityListener.Value.NewVelocityMinimum(minimum); }
		public void SetVelocityMultiplierCurrent(int multiplier) { velocityListener.Value = velocityListener.Value.NewMultiplierCurrent(multiplier); }
		public void SetVelocityMultiplierMaximum(int maximum) { velocityListener.Value = velocityListener.Value.NewMultiplierMaximum(maximum); }
		public void SetVelocityMultiplierEnabledMaximum(int enabledMaximum) { velocityListener.Value = velocityListener.Value.NewMultiplierEnabledMaximum(enabledMaximum); }
		#endregion
	}
}