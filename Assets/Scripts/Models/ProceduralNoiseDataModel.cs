using System;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

using LibNoise;

namespace LunraGames.SubLight.Models
{
	public class ProceduralNoiseDataModel : Model
	{
		public class DefinitionInstance
		{
			public IModule Module;
			public ProceduralNoiseDataModel Model;

			public float GetValue(
				Vector2 normalPosition
			)
			{
				return Module.GetValue(normalPosition.x, normalPosition.y, 0f);
			}
		}

		[JsonProperty] string noiseDataId;
		[JsonIgnore] public readonly ListenerProperty<string> NoiseDataId;

		[JsonProperty] string noiseAssetId;
		[JsonIgnore] public readonly ListenerProperty<string> NoiseAssetId;

		[JsonProperty] string key;
		[JsonIgnore] public readonly ListenerProperty<string> Key;

		[JsonProperty] int seedOffset;
		[JsonIgnore] public readonly ListenerProperty<int> SeedOffset;

		public ProceduralNoiseDataModel()
		{
			NoiseDataId = new ListenerProperty<string>(value => noiseDataId = value, () => noiseDataId);
			NoiseAssetId = new ListenerProperty<string>(value => noiseAssetId = value, () => noiseAssetId);
			Key = new ListenerProperty<string>(value => key = value, () => key);
			SeedOffset = new ListenerProperty<int>(value => seedOffset = value, () => seedOffset);
		}

		public DefinitionInstance CreateInstance(
			IModule module
		)
		{
			if (module == null) throw new ArgumentNullException("module");

			return new DefinitionInstance
			{
				Module = module,
				Model = this
			};
		}
	}
}