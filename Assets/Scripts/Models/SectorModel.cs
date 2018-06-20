using System.Linq;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class SectorModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] bool visited;
		[JsonProperty] UniversePosition position;
		[JsonProperty] SystemModel[] systems;

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
		public readonly ListenerProperty<SystemModel[]> Systems;
  		#endregion

		public SectorModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Visited = new ListenerProperty<bool>(value => visited = value, () => visited);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Systems = new ListenerProperty<SystemModel[]>(value => systems = value, () => systems);
		}

		public SystemModel GetSystem(UniversePosition position)
		{
			return Systems.Value.FirstOrDefault(s => s.Position.Value == position);
		}
	}
}