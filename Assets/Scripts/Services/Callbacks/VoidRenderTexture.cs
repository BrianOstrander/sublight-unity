using UnityEngine;

namespace LunraGames.SubLight
{
	public struct VoidRenderTexture
	{
		public readonly RenderTexture Texture;

		public VoidRenderTexture(RenderTexture texture)
		{
			Texture = texture;
		}
	}
}