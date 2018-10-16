using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class GalaxyInfoModel : SaveModel
	{
		public static class TextureNames
		{
			public const string BodyMap = "bodymap";
			public const string Preview = "preview";
			public const string Details = "details";
		}

		[JsonProperty] bool isPlayable;
		[JsonProperty] string galaxyId;
		[JsonProperty] string name;
		[JsonProperty] string description;

		[JsonProperty] UniversePosition playerStart;
		[JsonProperty] UniversePosition gameEnd;

		[JsonProperty] AnimationCurve bodyAdjustment;
		[JsonProperty] int minimumSectorBodies;
		[JsonProperty] int maximumSectorBodies;
		[JsonProperty] AnimationCurve sectorBodyChance;

		[JsonIgnore]
		public readonly ListenerProperty<bool> IsPlayable;
		[JsonIgnore]
		public readonly ListenerProperty<string> GalaxyId;
		[JsonIgnore]
		public readonly ListenerProperty<string> Name;
		[JsonIgnore]
		public readonly ListenerProperty<string> Description;

		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> PlayerStart;
		[JsonIgnore]
		public readonly ListenerProperty<UniversePosition> GameEnd;

		[JsonIgnore]
		public readonly ListenerProperty<AnimationCurve> BodyAdjustment;
		[JsonIgnore]
		public readonly ListenerProperty<int> MinimumSectorBodies;
		[JsonIgnore]
		public readonly ListenerProperty<int> MaximumSectorBodies;
		[JsonIgnore]
		public readonly ListenerProperty<AnimationCurve> SectorBodyChance;

		[JsonIgnore]
		public Texture2D BodyMap { get { return GetTexture(TextureNames.BodyMap); } }
		[JsonIgnore]
		public Texture2D Preview { get { return GetTexture(TextureNames.Preview); } }
		[JsonIgnore]
		public Texture2D Details { get { return GetTexture(TextureNames.Details); } }

		public GalaxyInfoModel()
		{
			SaveType = SaveTypes.GalaxyInfo;
			HasSiblingDirectory = true;

			IsPlayable = new ListenerProperty<bool>(value => isPlayable = value, () => isPlayable);
			GalaxyId = new ListenerProperty<string>(value => galaxyId = value, () => galaxyId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
			Description = new ListenerProperty<string>(value => description = value, () => description);

			PlayerStart = new ListenerProperty<UniversePosition>(value => playerStart = value, () => playerStart);
			GameEnd = new ListenerProperty<UniversePosition>(value => gameEnd = value, () => gameEnd);

			BodyAdjustment = new ListenerProperty<AnimationCurve>(value => bodyAdjustment = value, () => bodyAdjustment);
			MinimumSectorBodies = new ListenerProperty<int>(value => minimumSectorBodies = value, () => minimumSectorBodies);
			MaximumSectorBodies = new ListenerProperty<int>(value => maximumSectorBodies = value, () => maximumSectorBodies);
			SectorBodyChance = new ListenerProperty<AnimationCurve>(value => sectorBodyChance = value, () => sectorBodyChance);
		}
	}
}