using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SystemInstanceModel : Model
	{
		SystemModel activeSystem;

		readonly ListenerProperty<SystemModel> activeSystemListener;

		[JsonIgnore]
		public readonly ReadonlyProperty<SystemModel> ActiveSystem;

		[JsonIgnore]
		public int SystemIndex { get; private set; }
		[JsonIgnore]
		public bool HasSystem { get; private set; }

		public SystemInstanceModel(
			int systemIndex
		)
		{
			SystemIndex = systemIndex;

			ActiveSystem = new ReadonlyProperty<SystemModel>(value => activeSystem = value, () => activeSystem, out activeSystemListener);
		}

		public void SetSystem(SystemModel system)
		{
			HasSystem = system != null;
			activeSystemListener.Value = system;
		}
	}
}