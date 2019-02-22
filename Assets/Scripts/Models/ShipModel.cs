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

		[JsonProperty] TransitVelocity velocityold = TransitVelocity.Default;
		[JsonIgnore] ListenerProperty<TransitVelocity> velocityListenerold;
		[JsonIgnore] public readonly ReadonlyProperty<TransitVelocity> Velocityold;

		[JsonProperty] VelocityProfile velocity = VelocityProfile.Default;
		[JsonIgnore] public readonly ListenerProperty<VelocityProfile> Velocity;

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

			Velocityold = new ReadonlyProperty<TransitVelocity>(value => velocityold = value, () => velocityold, out velocityListenerold);
			Velocity = new ListenerProperty<VelocityProfile>(value => velocity = value, () => velocity);

			SystemIndex = new ListenerProperty<int>(value => systemIndex = value, () => systemIndex);
		}

		#region Utility
		public void SetRangeMinimum(float minimum) { rangeListener.Value = rangeListener.Value.NewMinimum(minimum); }

		// TODO: Somehow initialize these values in the rules initializition and listen for changes...

		public void SetVelocityMinimum(float minimum) { velocityListenerold.Value = velocityListenerold.Value.NewVelocityMinimum(minimum); }
		public void SetVelocityMultiplierCurrent(int multiplier) { velocityListenerold.Value = velocityListenerold.Value.NewMultiplierCurrent(multiplier); }
		public void SetVelocityMultiplierMaximum(int maximum) { velocityListenerold.Value = velocityListenerold.Value.NewMultiplierMaximum(maximum); }
		public void SetVelocityMultiplierEnabledMaximum(int enabledMaximum) { velocityListenerold.Value = velocityListenerold.Value.NewMultiplierEnabledMaximum(enabledMaximum); }
		#endregion
	}
}