using UnityEngine;

namespace LunraGames.SubLight
{
	public struct RenderLayerTextureBlock
	{
		public int Order;
		public Texture Texture;

		public string TextureKey { get { return ShaderConstants.RoomProjectionShared.GetLayer(Order); } }

		public RenderLayerTextureBlock(
			int order,
			Texture texture
		)
		{
			Order = order;
			Texture = texture;
		}
	}
}