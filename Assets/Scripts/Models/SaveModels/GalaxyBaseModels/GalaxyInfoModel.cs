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
	}
}