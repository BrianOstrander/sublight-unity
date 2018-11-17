using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SectorModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] int systemCount;
		[JsonProperty] bool visited;
		[JsonProperty] UniversePosition position;

		[JsonProperty] SystemModel[] systems = new SystemModel[0];

		readonly ListenerProperty<SystemModel[]> systemsListener;

		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Visited;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;
		[JsonIgnore]
		public readonly ReadonlyProperty<SystemModel[]> Systems;

		[JsonIgnore]
		public int SystemCount { get { return systemCount; } }

		public SectorModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Visited = new ListenerProperty<bool>(value => visited = value, () => visited);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);

			Systems = new ReadonlyProperty<SystemModel[]>(value => systems = value, () => systems, out systemsListener);
		}

		#region Utility
		public void SetSystems(SystemModel[] systems)
		{
			systemCount = systems.Length;
			systemsListener.Value = systems;
		}

		public SystemModel GetSystem(UniversePosition position)
		{
			return Systems.Value.FirstOrDefault(s => s.Position.Value == position);
		}

		public SystemModel GetSystem(int systemIndex)
		{
			if (systemIndex < SystemCount) return Systems.Value.FirstOrDefault(s => s.Index.Value == systemIndex);
			return null;
		}

		public BodyModel GetBody(UniversePosition position, int id)
		{
			return GetSystem(position).GetBody(id);
		}
		#endregion
	}
}