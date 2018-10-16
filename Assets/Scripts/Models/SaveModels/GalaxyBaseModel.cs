using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public abstract class GalaxyBaseModel : SaveModel
	{
		public static class TextureNames
		{
			public const string BodyMap = "bodymap";
			public const string Preview = "preview";
			public const string FullPreview = "fullpreview";
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

		public GalaxyBaseModel()
		{
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

		protected override void OnPrepareTexture(string name, Texture2D texture)
		{
			// TODO: This should probably be exposed by some interface, oh well...
			switch (name)
			{
				case TextureNames.Preview:
					texture.anisoLevel = 2;
					break;
			}
		}
	}
}