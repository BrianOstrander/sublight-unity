using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class GameModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] DayTime dayTime;
		[JsonProperty] float speed;
		[JsonProperty] UniverseModel universe;
		[JsonProperty] UniversePosition focusedSector;
		[JsonProperty] ShipModel ship;
		[JsonProperty] float destructionSpeed;
		[JsonProperty] float destructionRadius;

		/// <summary>
		/// The game seed.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		/// <summary>
		/// The day time.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<DayTime> DayTime;
		/// <summary>
		/// The speed of the ship, in universe units per day, whether or not
		/// it's curently in motion.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> Speed;
		/// <summary>
		/// The game universe.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<UniverseModel> Universe;
		/// <summary>
		/// The sector the camera is looking at.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> FocusedSector;
		/// <summary>
		/// The game ship.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<ShipModel> Ship;
		/// <summary>
		/// The speed at which the destruction expands, in universe units per
		/// day.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> DestructionSpeed;
		/// <summary>
		/// The total destruction radius, in universe units.
		/// </summary>
		[JsonIgnore]
		public readonly ListenerProperty<float> DestructionRadius;

		public GameModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			DayTime = new ListenerProperty<DayTime>(value => dayTime = value, () => dayTime);
			Speed = new ListenerProperty<float>(value => speed = value, () => speed);
			Universe = new ListenerProperty<UniverseModel>(value => universe = value, () => universe);
			FocusedSector = new ListenerProperty<UniversePosition>(value => focusedSector = value, () => focusedSector);
			Ship = new ListenerProperty<ShipModel>(value => ship = value, () => ship);
			DestructionSpeed = new ListenerProperty<float>(value => destructionSpeed = value, () => destructionSpeed);
			DestructionRadius = new ListenerProperty<float>(value => destructionRadius = value, () => destructionRadius);
		}
	}
}