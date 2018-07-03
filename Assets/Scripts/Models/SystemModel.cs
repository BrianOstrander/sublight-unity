using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class SystemModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] bool visited;
		[JsonProperty] UniversePosition position;
		[JsonProperty] string name;
		[JsonProperty] float rationsDetection;
		[JsonProperty] float rations;
		[JsonProperty] float fuelDetection;
		[JsonProperty] float fuel;

		/// <summary>
		/// Gets the type of the system.
		/// </summary>
		/// <value>The type of the save.</value>
		[JsonProperty]
		public SystemTypes SystemType { get; protected set; }

		#region Assigned
		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Visited;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;
		#endregion

		#region Procedural
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<float> Rations;
		[JsonIgnore]
		public readonly ListenerProperty<float> Fuel;
		[JsonIgnore]
		public readonly ListenerProperty<float> RationsDetection;
		[JsonIgnore]
		public readonly ListenerProperty<float> FuelDetection;
		#endregion

		public SystemModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Visited = new ListenerProperty<bool>(value => visited = value, () => visited);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);

			Name = new ListenerProperty<string>(value => name = value, () => name);
			Rations = new ListenerProperty<float>(value => rations = value, () => rations);
			Fuel = new ListenerProperty<float>(value => fuel = value, () => fuel);
			RationsDetection = new ListenerProperty<float>(value => rationsDetection = value, () => rationsDetection);
			FuelDetection = new ListenerProperty<float>(value => fuelDetection = value, () => fuelDetection);
		}
	}
}