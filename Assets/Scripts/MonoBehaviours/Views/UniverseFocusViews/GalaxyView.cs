using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GalaxyView : UniverseScaleView, IGalaxyView
	{
		[SerializeField]
		int renderQueue;
		[SerializeField]
		Color[] layerColors = new Color[0];
		[SerializeField]
		MeshRenderer[] layerMeshes = new MeshRenderer[0];

		public void SetGalaxy(Texture2D texture, Vector3 worldOrigin, float worldRadius)
		{
			for (var i = 0; i < layerMeshes.Length; i++)
			{
				var color = layerColors.Length == 0 ? Color.black : layerColors[Mathf.Min(i, layerColors.Length)];
				var mesh = layerMeshes[i];
				mesh.material.renderQueue = renderQueue;
				mesh.material.SetTexture(ShaderConstants.HoloGalaxy.LayerTexture, texture);
				mesh.material.SetInt(ShaderConstants.HoloGalaxy.Channel, i);
				mesh.material.SetColor(ShaderConstants.HoloGalaxy.ChannelColor, color);
				mesh.material.SetVector(ShaderConstants.HoloGalaxy.WorldOrigin, worldOrigin);
				mesh.material.SetFloat(ShaderConstants.HoloGalaxy.WorldRadius, worldRadius);
			}
		}

		public override void Reset()
		{
			base.Reset();

			SetGalaxy(null, Vector3.zero, 1f);
		}
	}

	public interface IGalaxyView : IUniverseScaleView
	{
		void SetGalaxy(Texture2D texture, Vector3 worldOrigin, float worldRadius);
	}
}