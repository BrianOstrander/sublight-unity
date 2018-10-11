using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class GalaxyInfoModel : SaveModel
	{
		[JsonProperty] bool isPlayable;
		[JsonProperty] string name;
		[JsonProperty] string galaxyId;

		[JsonProperty] AnimationCurve bodyAdjustment;
		[JsonProperty] int minimumSectorBodies;
		[JsonProperty] int maximumSectorBodies;
		[JsonProperty] AnimationCurve sectorBodyChance;
		[JsonProperty] UniversePosition playerStart;
		[JsonProperty] UniversePosition gameEnd;

		Texture2D bodyMap;
		Texture2D[] visualLayers;

		[JsonIgnore]
		public readonly ListenerProperty<bool> IsPlayable;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> GalaxyId;

		[JsonIgnore]
		public readonly ListenerProperty<AnimationCurve> BodyAdjustment;
		[JsonIgnore]
		public readonly ListenerProperty<int> MinimumSectorBodies;
		[JsonIgnore]
		public readonly ListenerProperty<int> MaximumSectorBodies;
		[JsonIgnore]
		public readonly ListenerProperty<AnimationCurve> SectorBodyChance;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> PlayerStart;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> GameEnd;

		[JsonIgnore]
		public readonly ListenerProperty<Texture2D> BodyMap;
		[JsonIgnore]
		public readonly ListenerProperty<Texture2D[]> VisualLayers;

		public GalaxyInfoModel()
		{
			SaveType = SaveTypes.GalaxyInfo;

			IsPlayable = new ListenerProperty<bool>(value => isPlayable = value, () => isPlayable);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			GalaxyId = new ListenerProperty<string>(value => galaxyId = value, () => galaxyId);

			BodyAdjustment = new ListenerProperty<AnimationCurve>(value => bodyAdjustment = value, () => bodyAdjustment);
			MinimumSectorBodies = new ListenerProperty<int>(value => minimumSectorBodies = value, () => minimumSectorBodies);
			MaximumSectorBodies = new ListenerProperty<int>(value => maximumSectorBodies = value, () => maximumSectorBodies);
			SectorBodyChance = new ListenerProperty<AnimationCurve>(value => sectorBodyChance = value, () => sectorBodyChance);
			PlayerStart = new ListenerProperty<UniversePosition>(value => playerStart = value, () => playerStart);
			GameEnd = new ListenerProperty<UniversePosition>(value => gameEnd = value, () => gameEnd);

			BodyMap = new ListenerProperty<Texture2D>(value => bodyMap = value, () => bodyMap);
			VisualLayers = new ListenerProperty<Texture2D[]>(value => visualLayers = value, () => visualLayers);
		}
	}
}