using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class HoloView : View, IHoloView
	{
		[SerializeField]
		Material holoMaterial;
		[SerializeField]
		MeshRenderer meshRenderer;

		Material currentMaterial;

		public RenderLayerTextureBlock[] LayerTextures
		{
			set
			{
				foreach (var block in value)
				{
					currentMaterial.SetTexture(ShaderConstants.HoloLayerShared.GetLayer(block.Order), block.Texture);
				}
			}
		}

		public RenderLayerPropertyBlock[] LayerProperties
		{
			set
			{
				foreach (var block in value)
				{
					currentMaterial.SetFloat(block.WeightKey, block.Weight);
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			currentMaterial = new Material(holoMaterial);
			meshRenderer.material = currentMaterial;
		}
	}

	public interface IHoloView : IView
	{
		RenderLayerTextureBlock[] LayerTextures { set; }
		RenderLayerPropertyBlock[] LayerProperties { set; }
	}
}