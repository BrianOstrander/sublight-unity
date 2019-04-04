using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class ShipModel : Model
	{
		#region Serialized
		[JsonProperty] UniversePosition position;
		[JsonIgnore] public readonly ListenerProperty<UniversePosition> Position;

		//[JsonProperty] TransitRange range = TransitRange.Default;
		//[JsonIgnore] ListenerProperty<TransitRange> rangeListener;
		//[JsonIgnore] public readonly ReadonlyProperty<TransitRange> Range;

		//[JsonProperty] VelocityProfileState velocity = VelocityProfileState.Default;
		//[JsonIgnore] public readonly ListenerProperty<VelocityProfileState> Velocity;

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
			//Range = new ReadonlyProperty<TransitRange>(value => range = value, () => range, out rangeListener);
			//Velocity = new ListenerProperty<VelocityProfileState>(value => velocity = value, () => velocity);
			SystemIndex = new ListenerProperty<int>(value => systemIndex = value, () => systemIndex);
		}

		#region Utility
		//public void SetRangeMinimum(float minimum) { rangeListener.Value = rangeListener.Value.NewMinimum(minimum); }
		#endregion
	}
}