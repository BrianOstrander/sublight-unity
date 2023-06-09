﻿using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GalaxyView : UniverseScaleView, IGalaxyView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		float ySeparation;
		[SerializeField]
		int renderQueue;
		[SerializeField]
		Color[] layerColors = new Color[0];
		[SerializeField]
		MeshRenderer[] layerMeshes = new MeshRenderer[0];
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		protected float TotalGalaxyHeight { get { return ySeparation * 3f; } }

		public virtual void SetGalaxy(Texture2D texture)
		{
			for (var i = 0; i < layerMeshes.Length; i++)
			{
				var color = layerColors.Length == 0 ? Color.black : layerColors[Mathf.Min(i, layerColors.Length)];
				var mesh = layerMeshes[i];
				mesh.transform.localPosition = new Vector3(0f, ySeparation * i, 0f);
				mesh.material.renderQueue = renderQueue;
				mesh.material.SetTexture(ShaderConstants.HoloGalaxy.LayerTexture, texture);
				mesh.material.SetInt(ShaderConstants.HoloGalaxy.Channel, i);
				mesh.material.SetColor(ShaderConstants.HoloGalaxy.ChannelColor, color);
				mesh.material.SetVector(ShaderConstants.HoloGalaxy.WorldOrigin, GridOrigin);
				mesh.material.SetFloat(ShaderConstants.HoloGalaxy.WorldRadius, GridRadius);
			}
		}

		protected override void OnOpacityStack(float opacity)
		{
			for (var i = 0; i < layerMeshes.Length; i++)
			{
				layerMeshes[i].material.SetFloat(ShaderConstants.HoloGalaxy.Alpha, opacity);
			}
		}

		public override void Reset()
		{
			base.Reset();

			SetGalaxy(null);
		}
	}

	public interface IGalaxyView : IUniverseScaleView
	{
		void SetGalaxy(Texture2D texture);
	}
}