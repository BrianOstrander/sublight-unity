using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ShipModel : Model
	{
		#region Assigned Values
		[JsonProperty] InventoryListModel inventory = new InventoryListModel();
		[JsonProperty] UniversePosition lastSystem;
		[JsonProperty] UniversePosition nextSystem;
		[JsonProperty] UniversePosition currentSystem;
		[JsonProperty] UniversePosition position;
		[JsonProperty] DayTime maximumNavigationTime;
		[JsonProperty] float fuelConsumption;
		[JsonProperty] float resourceDetection;

		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> LastSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> NextSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> CurrentSystem;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;

		/// <summary>
		/// The maximum length of a journey, in time.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<DayTime> MaximumNavigationTime;
		/// <summary>
		/// The fuel consumed per trip, multiplies speed.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> FuelConsumption;
		/// <summary>
		/// The likelyhood of detecting resources in a system.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> ResourceDetection;
		#endregion

		#region Derived Values
		[JsonProperty] TravelRadius travelRadius;
		[JsonProperty] float currentSpeed;
		[JsonProperty] float maximumSpeed;

		/// <summary>
		/// The travel radius of this ship, expressed as a ratio of speed and rations.
		/// </summary>
		[JsonIgnore]
		public readonly DerivedProperty<TravelRadius, float, DayTime, float, float> TravelRadius;
		/// <summary>
		/// The total speed of the ship, taking fuel consumption into account.
		/// </summary>
		[JsonIgnore]
		public readonly DerivedProperty<float, float, float> CurrentSpeed;
		/// <summary>
		/// The maximum speed, taking speed and current fuel consumption into account.
		/// </summary>
		[JsonIgnore]
		public readonly DerivedProperty<float, float, float> MaximumSpeed;
		#endregion

		#region Shortcuts
		[JsonIgnore]
		public InventoryListModel Inventory { get { return inventory; } }
		#endregion

		public ShipModel()
		{
			// Assigned Values

			LastSystem = new ListenerProperty<UniversePosition>(value => lastSystem = value, () => lastSystem);
			NextSystem = new ListenerProperty<UniversePosition>(value => nextSystem = value, () => nextSystem);
			CurrentSystem = new ListenerProperty<UniversePosition>(value => currentSystem = value, () => currentSystem);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			MaximumNavigationTime = new ListenerProperty<DayTime>(value => maximumNavigationTime = value, () => maximumNavigationTime);
			FuelConsumption = new ListenerProperty<float>(value => fuelConsumption = value, () => fuelConsumption);
			ResourceDetection = new ListenerProperty<float>(value => resourceDetection = value, () => resourceDetection);

			// Derived Values

			TravelRadius = new DerivedProperty<TravelRadius, float, DayTime, float, float>(
				value => travelRadius = value, 
				() => travelRadius,
				DeriveTravelRadius,
				Inventory.MaximumResources.Speed,
				MaximumNavigationTime,
				Inventory.UsableResources.Rations,
				FuelConsumption
			);

			CurrentSpeed = new DerivedProperty<float, float, float>(
				value => currentSpeed = value,
				() => currentSpeed,
				DeriveCurrentSpeed,
				Inventory.MaximumResources.Speed,
				FuelConsumption
			);

			MaximumSpeed = new DerivedProperty<float, float, float>(
				value => maximumSpeed = value,
				() => maximumSpeed,
				DeriveMaximumSpeed,
				Inventory.MaximumResources.Speed,
				Inventory.MaximumResources.Fuel
			);
		}

		#region Events
		float DeriveCurrentSpeed(float speed, float fuelConsumption)
		{
			return speed * fuelConsumption;
		}

		TravelRadius DeriveTravelRadius(
			float speed,
			DayTime maximumNavigationTime,
			float rations,
			float fuelConsumption
		)
		{
			var distance = maximumNavigationTime.TotalTime * DeriveCurrentSpeed(speed, fuelConsumption);
			// TODO: Find a better place for handling this weird range stuff?
			// Maybe not... this might be the correct place for it...
			return new TravelRadius(distance * 0.8f, distance * 0.9f, distance);
		}

		float DeriveMaximumSpeed(float speed, float maximumFuel)
		{
			return speed * maximumFuel;
		}
		#endregion
	}
}