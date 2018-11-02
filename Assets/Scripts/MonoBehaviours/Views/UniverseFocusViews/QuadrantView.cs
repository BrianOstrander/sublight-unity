using System.Linq;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class QuadrantView : UniverseScaleView, IQuadrantView
	{
		[SerializeField]
		bool[] channelEnabled;
		[SerializeField]
		float ySeparation;
		[SerializeField]
		int renderQueue;
		[SerializeField]
		Color[] layerColors = new Color[0];
		[SerializeField]
		MeshRenderer[] layerMeshes = new MeshRenderer[0];

		public void SetGalaxy(
			Texture2D previewTexture,
			Texture2D detailsTexture,
			Vector3 worldOrigin,
			float worldRadius
		)
		{
			var seperationIndex = 0;
			for (var i = 0; i < layerMeshes.Length; i++)
			{
				var isEnabled = channelEnabled[i];
				var mesh = layerMeshes[i];
				mesh.gameObject.SetActive(isEnabled);
				if (!isEnabled) continue;

				var color = layerColors.Length == 0 ? Color.black : layerColors[Mathf.Min(i, layerColors.Length)];
				mesh.transform.localPosition = new Vector3(0f, ySeparation * seperationIndex, 0f);
				mesh.material.renderQueue = renderQueue;
				mesh.material.SetTexture(ShaderConstants.HoloGalaxy.LayerTexture, detailsTexture);
				mesh.material.SetInt(ShaderConstants.HoloGalaxy.Channel, i);
				mesh.material.SetColor(ShaderConstants.HoloGalaxy.ChannelColor, color);
				mesh.material.SetVector(ShaderConstants.HoloGalaxy.WorldOrigin, worldOrigin);
				mesh.material.SetFloat(ShaderConstants.HoloGalaxy.WorldRadius, worldRadius);

				seperationIndex++;
			}
		}

		public override float Opacity
		{
			get { return base.Opacity; }

			set
			{
				base.Opacity = value;
				for (var i = 0; i < layerMeshes.Length; i++)
				{
					layerMeshes[i].material.SetFloat(ShaderConstants.HoloGalaxy.Alpha, value);
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			SetGalaxy(null, null, Vector3.zero, 1f);
		}
	}

	public interface IQuadrantView : IUniverseScaleView
	{
		void SetGalaxy(
			Texture2D previewTexture,
			Texture2D detailsTexture,
			Vector3 worldOrigin,
			float worldRadius
		);
	}
}