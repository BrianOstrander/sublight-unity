using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SpaceFarm.Models
{
	public class SectorModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] bool visited;
		[JsonProperty] UniversePosition position;

		[JsonProperty] StarModel[] stars = new StarModel[0];

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
			Systems = new ListenerProperty<SystemModel[]>(OnSetSystems, OnGetSystems);
		}

		#region Utility
		public SystemModel GetSystem(UniversePosition position)
		{
			return Systems.Value.FirstOrDefault(s => s.Position.Value == position);
		}
		#endregion

		#region Events
		void OnSetSystems(SystemModel[] newSystems)
		{
			var starList = new List<StarModel>();

			foreach (var system in newSystems)
			{
				switch(system.SystemType)
				{
					case SystemTypes.Star:
						starList.Add(system as StarModel);
						break;
					default:
						Debug.LogError("Unrecognized SystemType: " + system.SystemType);
						break;
				}
			}

			stars = starList.ToArray();
		}

		SystemModel[] OnGetSystems()
		{
			//return stars.Concat(stars).ToArray();
			return stars.ToArray();
		}
		#endregion
	}
}