using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class SystemModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] bool visited;
		[JsonProperty] UniversePosition position;
		[JsonProperty] string name;
		[JsonProperty] float rations;

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

		#region Derived
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<float> Rations;
		#endregion

		public SystemModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Visited = new ListenerProperty<bool>(value => visited = value, () => visited);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Rations = new ListenerProperty<float>(value => rations = value, () => rations);
		}
	}
}