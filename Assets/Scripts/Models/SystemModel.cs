using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class SystemModel : Model
	{
		[JsonProperty] int seed;
		[JsonProperty] bool visited;
		[JsonProperty] UniversePosition position;
		[JsonProperty] string name;
		[JsonProperty] string encounterId;
		[JsonProperty] int encounterBodyId = -1;
		[JsonProperty] BodyModel[] bodies = new BodyModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Visited;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> EncounterId;
		[JsonIgnore]
		public readonly ListenerProperty<int> EncounterBodyId;
		[JsonIgnore]
		public readonly ListenerProperty<BodyModel[]> Bodies;

		public SystemModel()
		{
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Visited = new ListenerProperty<bool>(value => visited = value, () => visited);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			EncounterBodyId = new ListenerProperty<int>(value => encounterBodyId = value, () => encounterBodyId);
			Bodies = new ListenerProperty<BodyModel[]>(value => bodies = value, () => bodies);
		}

		#region Utility
		[JsonIgnore]
		public bool HasEncounter { get { return !string.IsNullOrEmpty(EncounterId); } }
		[JsonIgnore]
		public bool HasBodyEncounter { get { return HasEncounter && EncounterBodyId.Value != -1; } }
		[JsonIgnore]
		public BodyModel BodyWithEncounter
		{
			get
			{
				if (!HasBodyEncounter) return null;
				// This should never return null...
				return Bodies.Value.FirstOrDefault(b => b.BodyId.Value == EncounterBodyId.Value);
			}
		}

		public BodyModel GetBody(int id)
		{
			return Bodies.Value.FirstOrDefault(b => b.BodyId.Value == id);
		}
		#endregion
	}
}