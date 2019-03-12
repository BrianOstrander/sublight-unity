using UnityEngine;

using Newtonsoft.Json;

using LibNoise;
using LibNoise.Modifiers;

using LunraGames.NumberDemon;

namespace LunraGames.SubLight.Models
{
	public class ProceduralNoiseModel : SaveModel
	{
		[JsonProperty] string proceduralNoiseId;
		[JsonIgnore] public readonly ListenerProperty<string> ProceduralNoiseId;

		[JsonProperty] string name;
		[JsonIgnore] public readonly ListenerProperty<string> Name;

		public ProceduralNoiseModel()
		{
			SaveType = SaveTypes.ProceduralNoise;
			ProceduralNoiseId = new ListenerProperty<string>(value => proceduralNoiseId = value, () => proceduralNoiseId);
			Name = new ListenerProperty<string>(value => name = value, () => name);
		}

		public IModule CreateInstance(int seed)
		{
			switch (ProceduralNoiseId.Value)
			{
				case "6701d03a-94a6-46fd-a460-f73e7144e476":
					return GetBasicGalaxy(seed);
				default:
					Debug.LogError("Procedural noise id \"" + ProceduralNoiseId.Value + "\" not stubbed out yet.");
					return null;
			}
		}

		IModule GetBasicGalaxy(int seed)
		{
			var demon = new Demon(seed);

			var perlin = new Perlin();

			perlin.Frequency = 0.2f;
			perlin.Lacunarity = 0f;
			perlin.NoiseQuality = NoiseQuality.Standard;
			perlin.OctaveCount = 1;
			perlin.Persistence = 0f;
			perlin.Seed = demon.NextInteger;

			var root = perlin;

			return new ScaleInput(
				root,
				10f,
				10f,
				10f
			);
		}
	}
}