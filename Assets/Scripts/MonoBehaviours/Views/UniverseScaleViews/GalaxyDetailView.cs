using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GalaxyDetailView : UniverseScaleView, IGalaxyDetailView
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField]
		bool[] channelEnabled;
		[SerializeField]
		float ySeparation;
		[SerializeField]
		int renderQueue;
		[SerializeField]
		Color[] layerColors;
		[SerializeField]
		AnimationCurve sliceOpacity;
		[SerializeField]
		float sliceOpacityMinimum;
		[SerializeField]
		AnimationCurve subSliceSeparation;
		[SerializeField]
		float subSliceRange;
		[SerializeField]
		GalaxySliceLeaf[] slices;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public void SetGalaxy(
			Texture2D previewTexture,
			Texture2D detailsTexture,
			Vector3 worldOrigin,
			float worldRadius
		)
		{
			var seperationIndex = 0;
			var subYDelta = ySeparation * subSliceRange;
			var opacityDelta = 1f - sliceOpacityMinimum;
			for (var i = 0; i < slices.Length; i++)
			{
				var slice = slices[i];
				var isEnabled = channelEnabled[i];

				slice.gameObject.SetActive(isEnabled);
				
				if (!isEnabled) continue;

				var color = layerColors.Length == 0 ? Color.black : layerColors[Mathf.Min(i, layerColors.Length)];
				var nextYSeparation = ySeparation * seperationIndex;

				slice.transform.localPosition = new Vector3(0f, nextYSeparation, 0f);

				for (var s = 0; s < slice.Meshes.Length; s++)
				{
					var progress = slice.Meshes.Length == 1 ? 0f : s / (slice.Meshes.Length - 1f);
					var sliceY = subSliceSeparation.Evaluate(progress) * subYDelta;
					var sliceColor = color.NewA(sliceOpacityMinimum + (sliceOpacity.Evaluate(progress) * opacityDelta));

					SetSlice(
						slice.Meshes[s],
						new Vector3(0f, sliceY, 0f),
						renderQueue,
						detailsTexture,
						i,
						sliceColor,
						worldOrigin,
						worldRadius
					);
				}

				seperationIndex++;
			}
		}

		void SetSlice(
			MeshRenderer mesh,
			Vector3 localPosition,
			int sliceRenderQueue,
			Texture2D detailsTexture,
			int channel,
			Color channelColor,
			Vector3 worldOrigin,
			float worldRadius
		)
		{
			mesh.transform.localPosition = localPosition;
			mesh.material.renderQueue = sliceRenderQueue;
			mesh.material.SetTexture(ShaderConstants.HoloGalaxy.LayerTexture, detailsTexture);
			mesh.material.SetInt(ShaderConstants.HoloGalaxy.Channel, channel);
			mesh.material.SetColor(ShaderConstants.HoloGalaxy.ChannelColor, channelColor);
			mesh.material.SetVector(ShaderConstants.HoloGalaxy.WorldOrigin, worldOrigin);
			mesh.material.SetFloat(ShaderConstants.HoloGalaxy.WorldRadius, worldRadius);
		}

		protected override void OnOpacityStack(float opacity)
		{
			foreach (var slice in slices)
			{
				foreach (var subSlice in slice.Meshes) subSlice.material.SetFloat(ShaderConstants.HoloGalaxy.Alpha, opacity);
			}
		}

		public override void Reset()
		{
			base.Reset();

			SetGalaxy(null, null, Vector3.zero, 1f);
		}
	}

	public interface IGalaxyDetailView : IUniverseScaleView
	{
		void SetGalaxy(
			Texture2D previewTexture,
			Texture2D detailsTexture,
			Vector3 worldOrigin,
			float worldRadius
		);
	}
}