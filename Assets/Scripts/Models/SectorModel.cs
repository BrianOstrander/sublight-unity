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

		[JsonProperty] CelestialSystemModel[] celestials = new CelestialSystemModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Visited;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;

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
			var celestialList = new List<CelestialSystemModel>();

			foreach (var system in newSystems)
			{
				switch(system.SystemType)
				{
					case SystemTypes.Celestial:
						celestialList.Add(system as CelestialSystemModel);
						break;
					default:
						Debug.LogError("Unrecognized SystemType: " + system.SystemType);
						break;
				}
			}

			celestials = celestialList.ToArray();
		}

		SystemModel[] OnGetSystems()
		{
			return celestials.ToArray();
		}
		#endregion
	}
}