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

		[SerializeField]
		Vector2 glowIntensityRange;
		[SerializeField]
		AnimationCurve glowIntensity;
		[SerializeField]
		Vector2 gridIntensityRange;
		[SerializeField]
		AnimationCurve gridIntensity;

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

		public Vector2 GridOffset
		{
			set
			{
				irisMesh.material.SetVector(ShaderConstants.RoomIrisGrid.Offset, value);
			}
		}

		public float CameraPitch
		{
			set
			{
				var glow = glowIntensityRange.x + ((glowIntensityRange.y - glowIntensityRange.x) * glowIntensity.Evaluate(value));
				var grid = gridIntensityRange.x + ((gridIntensityRange.y - gridIntensityRange.x) * gridIntensity.Evaluate(value));

				glowMesh.material.SetFloat(ShaderConstants.RoomIrisGlow.GlowIntensity, glow);
				irisMesh.material.SetFloat(ShaderConstants.RoomIrisGrid.GridIntensity, grid);
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

			GridOffset = Vector2.zero;
			CameraPitch = 0f;
			HoloColor = Color.white;
		}

	}

	public interface IHoloView : IView, IHoloColorView
	{
		RenderLayerTextureBlock[] LayerTextures { set; }
		RenderLayerPropertyBlock[] LayerProperties { set; }
		Vector2 GridOffset { set; }
		float CameraPitch { set; }
	}
}