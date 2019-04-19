using Newtonsoft.Json;

using LunraGames.NumberDemon;

namespace LunraGames.SubLight.Models
{
	public class SectorModel : Model
	{
		public enum SpecifiedPlacements
		{
			Unknown = 0,
			Position = 10,
			PositionList = 20
		}

		public static class Seeds
		{
			public static int Position(int seed) { return DemonUtility.CantorPairs(seed, 1); }
		}

		[JsonProperty] string name;
		[JsonIgnore] public readonly ListenerProperty<string> Name;

		[JsonProperty] int seed;
		[JsonIgnore] public readonly ListenerProperty<int> Seed;

		[JsonProperty] bool visited;
		[JsonIgnore] public readonly ListenerProperty<bool> Visited;

		[JsonProperty] bool specified;
		[JsonIgnore] public readonly ListenerProperty<bool> Specified;

		[JsonProperty] int systemCount;
		[JsonIgnore] public readonly ListenerProperty<int> SystemCount;

		[JsonProperty] SystemModel[] systems = new SystemModel[0];
		[JsonIgnore] public readonly ListenerProperty<SystemModel[]> Systems;

		[JsonProperty] SpecifiedPlacements specifiedPlacement;
		[JsonIgnore] public readonly ListenerProperty<SpecifiedPlacements> SpecifiedPlacement;

		[JsonProperty] UniversePosition position;
		[JsonIgnore] public readonly ListenerProperty<UniversePosition> Position;

		[JsonProperty] UniversePosition[] positionList = new UniversePosition[0];
		[JsonIgnore] public readonly ListenerProperty<UniversePosition[]> PositionList;

		// I think this is just a hack for now.
		public int sectorOffset;

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
			SystemCount = new ListenerProperty<int>(value => systemCount = value, () => systemCount);
			Systems = new ListenerProperty<SystemModel[]>(value => systems = value, () => systems);

			SpecifiedPlacement = new ListenerProperty<SpecifiedPlacements>(value => specifiedPlacement = value, () => specifiedPlacement);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position, OnPosition);
			PositionList = new ListenerProperty<UniversePosition[]>(value => positionList = value, () => positionList);
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