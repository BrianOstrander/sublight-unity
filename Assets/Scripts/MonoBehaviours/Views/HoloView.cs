using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class HoloView : View, IHoloView
	{
		[SerializeField]
		MeshRenderer projectionMesh;
		[SerializeField]
		MeshRenderer glowMesh;
		[SerializeField]
		MeshRenderer irisMesh;

		[SerializeField]
		Color baseGlowColor;
		[SerializeField]
		Color baseIrisColor;

		public RenderLayerTextureBlock[] LayerTextures
		{
			set
			{
				foreach (var block in value)
				{
					projectionMesh.material.SetTexture(ShaderConstants.RoomProjectionShared.GetLayer(block.Order), block.Texture);
				}
			}
		}

		public RenderLayerPropertyBlock[] LayerProperties
		{
			set
			{
				foreach (var block in value)
				{
					projectionMesh.material.SetFloat(block.WeightKey, block.Weight);
				}
			}
		}

		public Color HoloColor
		{
			set
			{
				glowMesh.material.SetColor(ShaderConstants.RoomIrisGlow.GlowColor, baseGlowColor.NewHsva(value.GetH(), value.GetS()));
				irisMesh.material.SetColor(ShaderConstants.RoomIrisGrid.GridColor, baseIrisColor.NewHsva(value.GetH(), value.GetS()));
			}
		}

		public override void Reset()
		{
			base.Reset();

			HoloColor = Color.white;
		}
	}

	public interface IHoloView : IView, IHoloColorView
	{
		RenderLayerTextureBlock[] LayerTextures { set; }
		RenderLayerPropertyBlock[] LayerProperties { set; }
	}
}