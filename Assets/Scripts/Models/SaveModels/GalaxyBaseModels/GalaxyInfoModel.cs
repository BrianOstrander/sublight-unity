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

		UniversePosition? galaxySize;
		/// <summary>
		/// Gets the size of the galaxy in sectors.
		/// </summary>
		/// <value>The size of the galaxy.</value>
		[JsonIgnore]
		public UniversePosition GalaxySize
		{
			get
			{
				if (galaxySize.HasValue) return galaxySize.Value;
				var largestDimension = Mathf.Max(BodyMap.width, BodyMap.height);
				return (galaxySize = new UniversePosition(new Vector3(largestDimension, largestDimension, largestDimension))).Value;
			}
		}

		public GalaxyInfoModel()
		{
			SaveType = SaveTypes.GalaxyInfo;

			SiblingBehaviour = SiblingBehaviours.All;
		}
	}
}