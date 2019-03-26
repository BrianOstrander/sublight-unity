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
			public static int Resources(int seed) { return DemonUtility.CantorPairs(seed, 2); }
		}

		[JsonProperty] UniversePosition position;
		[JsonProperty] SystemClassifications primaryClassification;
		[JsonProperty] BodyModel[] bodies = new BodyModel[0];

		[JsonProperty] SpecifiedEncounterEntry[] specifiedEncounters = new SpecifiedEncounterEntry[0];

		[JsonIgnore] public readonly ListenerProperty<int> Index;
		[JsonIgnore] public readonly ListenerProperty<int> Seed;
		[JsonIgnore] public readonly ListenerProperty<bool> Visited;
		[JsonIgnore] public readonly ListenerProperty<bool> Specified;
		[JsonIgnore] public readonly ListenerProperty<bool> PlayerBegin;
		[JsonIgnore] public readonly ListenerProperty<bool> PlayerEnd;
		[JsonIgnore] public readonly ListenerProperty<string> Name;
		[JsonIgnore] public readonly ListenerProperty<string> SecondaryClassification;
		[JsonIgnore] public readonly ListenerProperty<Color> IconColor;
		[JsonIgnore] public readonly ListenerProperty<float> IconScale;

		[JsonIgnore] public readonly ListenerProperty<UniversePosition> Position;
		[JsonIgnore] public readonly ListenerProperty<SystemClassifications> PrimaryClassification;
		[JsonIgnore] public readonly ListenerProperty<BodyModel[]> Bodies;

		[JsonIgnore]
		public readonly ListenerProperty<SpecifiedEncounterEntry[]> SpecifiedEncounters;

		[JsonIgnore]
		public bool IsGenerated { get; set; }

		[JsonProperty] KeyValueListModel keyValues = new KeyValueListModel();
		[JsonIgnore] public KeyValueListModel KeyValues { get { return keyValues; } }

		[JsonIgnore]
		public string ShrunkPosition { get { return UniversePosition.Shrink(Position.Value, Index.Value); } }

		public SystemModel()
		{
			Index = new ListenerProperty<int>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.Index, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.Index)
			);
			Seed = new ListenerProperty<int>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.Seed, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.Seed)
			);
			Visited = new ListenerProperty<bool>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.Visited, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.Visited)
			);
			PlayerBegin = new ListenerProperty<bool>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.PlayerBegin, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.PlayerBegin)
			);
			PlayerEnd = new ListenerProperty<bool>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.PlayerEnd, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.PlayerEnd)
			);
			Specified = new ListenerProperty<bool>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.Specified, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.Specified)
			);
			Name = new ListenerProperty<string>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.Name, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.Name)
			);
			SecondaryClassification = new ListenerProperty<string>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.ClassificationSecondary, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.ClassificationSecondary)
			);
			IconColor = new ListenerProperty<Color>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.IconColor, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.IconColor)
			);
			IconScale = new ListenerProperty<float>(
				value => KeyValues.Set(KeyDefines.CelestialSystem.IconScale, value),
				() => KeyValues.Get(KeyDefines.CelestialSystem.IconScale)
			);

			Position = new ListenerProperty<UniversePosition>(value => position = value, () => position);
			PrimaryClassification = new ListenerProperty<SystemClassifications>(value => primaryClassification = value, () => primaryClassification);
			Bodies = new ListenerProperty<BodyModel[]>(value => bodies = value, () => bodies);

			SpecifiedEncounters = new ListenerProperty<SpecifiedEncounterEntry[]>(value => specifiedEncounters = value, () => specifiedEncounters);
		}

		#region Utility

		#endregion
	}
}