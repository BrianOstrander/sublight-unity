using System.Linq;

using Newtonsoft.Json;

using UnityEngine;

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
		[JsonProperty] Color iconColor;
		[JsonProperty] float iconScale;
		[JsonProperty] string encounterId;
		[JsonProperty] int encounterBodyId = -1;
		[JsonProperty] BodyModel[] bodies = new BodyModel[0];

		[JsonProperty] string specifiedEncounterId;

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
		public readonly ListenerProperty<Color> IconColor;
		[JsonIgnore]
		public readonly ListenerProperty<float> IconScale;
		[JsonIgnore]
		public readonly ListenerProperty<string> EncounterId; // TODO: probably delete this?
		[JsonIgnore]
		public readonly ListenerProperty<int> EncounterBodyId; // TODO: probably delete this?
		[JsonIgnore]
		public readonly ListenerProperty<BodyModel[]> Bodies;

		[JsonIgnore]
		public readonly ListenerProperty<string> SpecifiedEncounterId;

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
			IconColor = new ListenerProperty<Color>(value => iconColor = value, () => iconColor);
			IconScale = new ListenerProperty<float>(value => iconScale = value, () => iconScale);
			EncounterId = new ListenerProperty<string>(value => encounterId = value, () => encounterId);
			EncounterBodyId = new ListenerProperty<int>(value => encounterBodyId = value, () => encounterBodyId);
			Bodies = new ListenerProperty<BodyModel[]>(value => bodies = value, () => bodies);

			SpecifiedEncounterId = new ListenerProperty<string>(value => specifiedEncounterId = value, () => specifiedEncounterId);
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