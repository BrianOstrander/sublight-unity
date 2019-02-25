using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class GalaxyDistantModel : GalaxyBaseModel
	{
		[JsonIgnore]
		public Texture2D FullPreview { get { return GetTexture(TextureNames.FullPreview); } }

		public GalaxyDistantModel()
		{
			SaveType = SaveTypes.GalaxyDistant;

			SiblingBehaviour = SiblingBehaviours.Specified;
			AddSiblings(TextureNames.FullPreview);
		}
	}
}