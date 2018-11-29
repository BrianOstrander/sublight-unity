﻿using System.Linq;

using Newtonsoft.Json;

using LunraGames.NumberDemon;

namespace LunraGames.SubLight.Models
{
	public class SystemModel : Model
	{
		public static class Seeds
		{
			public static int Position(int seed) { return DemonUtility.CantorPairs(seed, 1); }
		}

		[JsonProperty] int index;
		[JsonProperty] int seed;
		[JsonProperty] bool visited;
		[JsonProperty] bool specified;
		[JsonProperty] bool playerBegin;
		[JsonProperty] bool playerEnd;
		[JsonProperty] UniversePosition position;
		[JsonProperty] string name;
		[JsonProperty] SystemClassifications primaryClassification;
		[JsonProperty] string secondaryClassification;
		[JsonProperty] string encounterId;
		[JsonProperty] int encounterBodyId = -1;
		[JsonProperty] BodyModel[] bodies = new BodyModel[0];

		[JsonIgnore]
		public readonly ListenerProperty<int> Index;
		[JsonIgnore]
		public readonly ListenerProperty<int> Seed;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Visited;
		[JsonIgnore]
		public readonly ListenerProperty<bool> Specified;
		[JsonIgnore]
		public readonly ListenerProperty<bool> PlayerBegin;
		[JsonIgnore]
		public readonly ListenerProperty<bool> PlayerEnd;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> Position;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<SystemClassifications> PrimaryClassification;
		[JsonIgnore]
		public readonly ListenerProperty<string> SecondaryClassification;
		[JsonIgnore]
		public readonly ListenerProperty<string> EncounterId; // TODO: probably delete this?
		[JsonIgnore]
		public readonly ListenerProperty<int> EncounterBodyId; // TODO: probably delete this?
		[JsonIgnore]
		public readonly ListenerProperty<BodyModel[]> Bodies;

		[JsonIgnore]
		public bool IsGenerated { get; set; }

		public SystemModel()
		{
			Index = new ListenerProperty<int>(value => index = value, () => index);
			Seed = new ListenerProperty<int>(value => seed = value, () => seed);
			Visited = new ListenerProperty<bool>(value => visited = value, () => visited);
			PlayerBegin = new ListenerProperty<bool>(value => playerBegin = value, () => playerBegin);
			PlayerEnd = new ListenerProperty<bool>(value => playerEnd = value, () => playerEnd);
			Specified = new ListenerProperty<bool>(value => specified = value, () => specified);
			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			PrimaryClassification = new ListenerProperty<SystemClassifications>(value => primaryClassification = value, () => primaryClassification);
			SecondaryClassification = new ListenerProperty<string>(value => secondaryClassification = value, () => secondaryClassification);
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
		#endregion
	}
}