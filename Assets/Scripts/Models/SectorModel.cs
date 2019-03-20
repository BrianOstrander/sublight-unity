using Newtonsoft.Json;

using LunraGames.NumberDemon;

namespace LunraGames.SubLight.Models
{
	public class SectorModel : Model
	{
		public static class Seeds
		{
			public static int Position(int seed) { return DemonUtility.CantorPairs(seed, 1); }
		}

		[JsonProperty] string name;
		[JsonProperty] int seed;
		[JsonProperty] bool visited;
		[JsonProperty] bool specified;
		[JsonProperty] UniversePosition position;
		[JsonProperty] int systemCount;
		[JsonProperty] SystemModel[] systems = new SystemModel[0];

		public int sectorOffset;

		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Visited;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Specified;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;
		[JsonIgnore]
		public readonly ListenerProperty<int> SystemCount;
		[JsonIgnore]
		public readonly ListenerProperty<SystemModel[]> Systems;

		bool isGenerated;
		[JsonIgnore]
		public bool IsGenerated
		{
			get { return isGenerated || Visited.Value || Specified.Value; }
			set { isGenerated = value; }
		}

		public SectorModel()
		{
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Visited = new ListenerProperty<bool>(value => visited = value, () => visited);
			Specified = new ListenerProperty<bool>(value => specified = value, () => specified);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position, OnPosition);
			SystemCount = new ListenerProperty<int>(value => systemCount = value, () => systemCount);
			Systems = new ListenerProperty<SystemModel[]>(value => systems = value, () => systems);
		}

		#region Events
		void OnPosition(UniversePosition position)
		{
			foreach (var system in Systems.Value)
			{
				system.Position.Value = new UniversePosition(position.SectorInteger, system.Position.Value.Local);
			}
		}
 		#endregion 
	}
}