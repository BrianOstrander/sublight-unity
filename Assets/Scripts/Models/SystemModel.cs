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
		[JsonProperty] BodyModel[] bodies = new BodyModel[0];

		[JsonProperty] SpecifiedEncounterEntry[] specifiedEncounters = new SpecifiedEncounterEntry[0];

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
		public readonly ListenerProperty<BodyModel[]> Bodies;

		[JsonIgnore]
		public readonly ListenerProperty<SpecifiedEncounterEntry[]> SpecifiedEncounters;

		[JsonIgnore]
		public bool IsGenerated { get; set; }

		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore] public KeyValueListModel KeyValues { get { return keyValues; } }

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
			Bodies = new ListenerProperty<BodyModel[]>(value => bodies = value, () => bodies);

			SpecifiedEncounters = new ListenerProperty<SpecifiedEncounterEntry[]>(value => specifiedEncounters = value, () => specifiedEncounters);
		}

		#region Utility

		#endregion
	}
}