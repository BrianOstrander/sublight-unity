using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class ShipModel : Model
	{
		[JsonProperty] UniversePosition lastSystem;
		[JsonProperty] UniversePosition nextSystem;
		[JsonProperty] UniversePosition currentSystem;
		[JsonProperty] UniversePosition position;
		[JsonProperty] float speed;
		[JsonProperty] float rationConsumption;
		[JsonProperty] float rations;
		[JsonProperty] TravelRadius travelRadius;

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
		/// The years worth of rations on board.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Rations;
		/// <summary>
		/// The travel radius of this ship, expressed as a ratio of speed and rations.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<TravelRadius> TravelRadius;

		public ShipModel()
		{
			LastSystem = new ListenerProperty<UniversePosition>(value => lastSystem = value, () => lastSystem);
			NextSystem = new ListenerProperty<UniversePosition>(value => nextSystem = value, () => nextSystem);
			CurrentSystem = new ListenerProperty<UniversePosition>(value => currentSystem = value, () => currentSystem);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Speed = new ListenerProperty<float>(value => speed = value, () => speed);
			RationConsumption = new ListenerProperty<float>(value => rationConsumption = value, () => rationConsumption);
			Rations = new ListenerProperty<float>(value => rations = value, () => rations);
			TravelRadius = new ListenerProperty<TravelRadius>(value => travelRadius = value, () => travelRadius);
		}
	}
}