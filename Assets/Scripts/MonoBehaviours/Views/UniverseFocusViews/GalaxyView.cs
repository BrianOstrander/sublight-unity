using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GalaxyView : UniverseScaleView, IGalaxyView
	{
		[SerializeField]
		Color[] layerColors = new Color[0];
		[SerializeField]
		MeshRenderer[] layerMeshes = new MeshRenderer[0];

		public Texture2D GalaxyPreview
		{
			set
			{
				for (var i = 0; i < layerMeshes.Length; i++)
				{
					var color = layerColors.Length == 0 ? Color.black : layerColors[Mathf.Min(i, layerColors.Length)];
					var mesh = layerMeshes[i];
					mesh.material.SetTexture(ShaderConstants.HoloGalaxyPreviewBasic.LayerTexture, value);
					mesh.material.SetInt(ShaderConstants.HoloGalaxyPreviewBasic.Channel, i);
					mesh.material.SetColor(ShaderConstants.HoloGalaxyPreviewBasic.ChannelColor, color);
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			GalaxyPreview = null;
		}
	}

	public interface IGalaxyView : IUniverseScaleView
	{
		Texture2D GalaxyPreview { set; }
	}
}