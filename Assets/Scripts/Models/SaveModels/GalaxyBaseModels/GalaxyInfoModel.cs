using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class GalaxyInfoModel : GalaxyBaseModel
	{
		[JsonIgnore]
		public Texture2D BodyMap { get { return GetTexture(TextureNames.BodyMap); } }
		[JsonIgnore]
		public Texture2D FullPreview { get { return GetTexture(TextureNames.FullPreview); } }
		[JsonIgnore]
		public Texture2D Details { get { return GetTexture(TextureNames.Details); } }

		public GalaxyInfoModel()
		{
			SaveType = SaveTypes.GalaxyInfo;

			SiblingBehaviour = SiblingBehaviours.All;
		}

		public TexturePixelCache GetBodyMapPixel(UniversePosition sectorPosition)
		{
			return GetPixel(BodyMap, sectorPosition);
		}

		TexturePixelCache GetPixel(Texture2D texture, UniversePosition sectorPosition)
		{
			var normal = UniversePosition.NormalizedSector(GalaxySize, sectorPosition);
			var x = Mathf.FloorToInt(normal.x * (texture.width - 1));
			var y = Mathf.FloorToInt(normal.y * (texture.height - 1));
			return new TexturePixelCache(sectorPosition.SectorInteger.x, sectorPosition.SectorInteger.y, texture.GetPixel(x, y));
		}
	}
}