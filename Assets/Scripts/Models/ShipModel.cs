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
		readonly ListenerProperty<int> systemIndexListener;
		[JsonIgnore] public readonly ReadonlyProperty<int> SystemIndex;
		#endregion

		#region NonSerialized
		SystemModel currentSystem;
		readonly ListenerProperty<SystemModel> currentSystemListener;
		[JsonIgnore]
		public ReadonlyProperty<SystemModel> CurrentSystem;
		#endregion

		public ShipModel()
		{
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Range = new ReadonlyProperty<TransitRange>(value => range = value, () => range, out rangeListener);
			Velocity = new ReadonlyProperty<TransitVelocity>(value => velocity = value, () => velocity, out velocityListener);

			//SystemIndex = new ListenerProperty<int>(value => systemIndex = value, () => systemIndex);
			//CurrentSystem = new ListenerProperty<SystemModel>(value => currentSystem = value, () => currentSystem);
			SystemIndex = new ReadonlyProperty<int>(value => systemIndex = value, () => systemIndex, out systemIndexListener);
			CurrentSystem = new ReadonlyProperty<SystemModel>(value => currentSystem = value, () => currentSystem, out currentSystemListener);
		}

		#region Utility
		public void SetRangeMinimum(float minimum) { rangeListener.Value = rangeListener.Value.NewMinimum(minimum); }

		public void SetCurrentSystem(SystemModel system)
		{
			currentSystemListener.Value = system;
			systemIndexListener.Value = system == null ? -1 : system.Index.Value; 
		}

		public void SetVelocityMinimum(float minimum) { velocityListener.Value = velocityListener.Value.NewVelocityMinimum(minimum); }
		public void SetVelocityMultiplierCurrent(int multiplier) { velocityListener.Value = velocityListener.Value.NewMultiplierCurrent(multiplier); }
		public void SetVelocityMultiplierMaximum(int maximum) { velocityListener.Value = velocityListener.Value.NewMultiplierMaximum(maximum); }
		public void SetVelocityMultiplierEnabledMaximum(int enabledMaximum) { velocityListener.Value = velocityListener.Value.NewMultiplierEnabledMaximum(enabledMaximum); }
		#endregion
	}
}