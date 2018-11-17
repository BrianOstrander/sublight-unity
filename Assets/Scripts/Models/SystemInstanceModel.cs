using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SystemInstanceModel : Model
	{
		UniverseModel universe;
		UniversePosition sector;
		SystemModel activeSystem;

		readonly ListenerProperty<SystemModel> activeSystemListener;

		[JsonIgnore]
		public readonly ReadonlyProperty<SystemModel> ActiveSystem;

		[JsonIgnore]
		public int SystemIndex { get; private set; }
		[JsonIgnore]
		public bool HasSystem { get; private set; }

		public SystemInstanceModel(UniverseModel universe, int systemIndex)
		{
			this.universe = universe;
			SystemIndex = systemIndex;

			ActiveSystem = new ReadonlyProperty<SystemModel>(value => activeSystem = value, () => activeSystem, out activeSystemListener);
		}

		public void SetSector(UniversePosition sector)
		{
			this.sector = sector;
			var newSystem = universe.GetSystem(sector, SystemIndex);
			HasSystem = newSystem != null;
			activeSystemListener.Value = newSystem;
		}
	}
}