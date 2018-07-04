using UnityEngine;

namespace LunraGames.SpaceFarm
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