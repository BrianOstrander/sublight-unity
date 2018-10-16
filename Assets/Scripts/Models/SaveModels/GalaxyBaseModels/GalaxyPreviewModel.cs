using UnityEngine;

using Newtonsoft.Json;

namespace LunraGames.SubLight.Models
{
	public class GalaxyPreviewModel : GalaxyBaseModel
	{
		[JsonIgnore]
		public Texture2D Preview { get { return GetTexture(TextureNames.Preview); } }

		public GalaxyPreviewModel()
		{
			SaveType = SaveTypes.GalaxyPreview;

			SiblingBehaviour = SiblingBehaviours.Specified;
			AddSiblings(TextureNames.Preview);
		}
	}
}