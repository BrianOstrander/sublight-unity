using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SectorModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] bool visited;
		[JsonProperty] UniversePosition position;

		[JsonProperty] SystemModel[] systems = new SystemModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Visited;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;
		[JsonIgnore]
		public readonly ListenerProperty<SystemModel[]> Systems;

		public SectorModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Visited = new ListenerProperty<bool>(value => visited = value, () => visited);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Systems = new ListenerProperty<SystemModel[]>(value => systems = value, () => systems);
		}

		#region Utility
		public SystemModel GetSystem(UniversePosition position)
		{
			return Systems.Value.FirstOrDefault(s => s.Position.Value == position);
		}

		public BodyModel GetBody(UniversePosition position, int id)
		{
			return GetSystem(position).GetBody(id);
		}
		#endregion
	}
}