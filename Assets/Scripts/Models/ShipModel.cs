using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ShipModel : Model
	{
		#region Assigned Values
		[JsonProperty] UniversePosition lastSystem;
		[JsonProperty] UniversePosition nextSystem;
		[JsonProperty] UniversePosition currentSystem;
		[JsonProperty] UniversePosition position;
		[JsonProperty] float speed;
		[JsonProperty] float rationConsumption;
		[JsonProperty] float rations;
		[JsonProperty] float fuel;
		[JsonProperty] float fuelConsumption;

		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> LastSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> NextSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> CurrentSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;

		/// <summary>
		/// Basically the speed of the ship, expressed in universe units per day.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Speed;
		/// <summary>
		/// The ration consumption rate in rations per day.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> RationConsumption;
		/// <summary>
		/// The years worth of rations on board, assuming a person uses 1 ration
		/// a year.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Rations;
		/// <summary>
		/// The fuel onboard, multiplies speed.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Fuel;
		/// <summary>
		/// The fuel consumed per trip, multiplies speed.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> FuelConsumption;
		#endregion

		#region Derived Values
		[JsonProperty] TravelRadius travelRadius;
		[JsonProperty] float speedTotal;

		/// <summary>
		/// The travel radius of this ship, expressed as a ratio of speed and rations.
		/// </summary>
		[JsonIgnore]
		public readonly DerivedProperty<TravelRadius, float, float, float, float> TravelRadius;
		/// <summary>
		/// The total speed of the ship, taking fuel consumption into account.
		/// </summary>
		[JsonIgnore]
		public readonly DerivedProperty<float, float, float> SpeedTotal;
		#endregion

		public ShipModel()
		{
			// Assigned Values

			LastSystem = new ListenerProperty<UniversePosition>(value => lastSystem = value, () => lastSystem);
			NextSystem = new ListenerProperty<UniversePosition>(value => nextSystem = value, () => nextSystem);
			CurrentSystem = new ListenerProperty<UniversePosition>(value => currentSystem = value, () => currentSystem);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Speed = new ListenerProperty<float>(value => speed = value, () => speed);
			RationConsumption = new ListenerProperty<float>(value => rationConsumption = value, () => rationConsumption);
			Rations = new ListenerProperty<float>(value => rations = value, () => rations);
			Fuel = new ListenerProperty<float>(value => fuel = value, () => fuel);
			FuelConsumption = new ListenerProperty<float>(value => fuelConsumption = value, () => fuelConsumption);

			// Derived Values

			TravelRadius = new DerivedProperty<TravelRadius, float, float, float, float>(
				value => travelRadius = value, 
				() => travelRadius,
				DeriveTravelRadius,
				Speed,
				RationConsumption,
				Rations,
				FuelConsumption
			);

			SpeedTotal = new DerivedProperty<float, float, float>(
				value => speedTotal = value,
				() => speedTotal,
				DeriveSpeedTotal,
				Speed,
				FuelConsumption
			);
		}

		#region Events
		float DeriveSpeedTotal(float speed, float fuelConsumption)
		{
			return speed * fuelConsumption;
		}

		TravelRadius DeriveTravelRadius(
			float speed,
			float rationConsumption,
			float rations,
			float fuelConsumption
		)
		{
			var rationDuration = DayTime.FromDayNormal(rations / rationConsumption);
			var rationDistance = rationDuration.TotalTime * DeriveSpeedTotal(speed, fuelConsumption);
			// TODO: Find a better place for handling this weird range stuff?
			// Maybe not... this might be the correct place for it...
			return new TravelRadius(rationDistance * 0.8f, rationDistance * 0.9f, rationDistance);
		}
		//float GetTotalSpeed() { return Speed.Value * FuelConsumption.Value; }
		//void SetTotalSpeed(float totalSpeed) { Speed.Value}
  		#endregion
	}
}