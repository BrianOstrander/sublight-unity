using System;

using UnityEngine;

namespace LunraGames.SubLight.Views
{
	public class GamemodePortalView : View, IGamemodePortalView
	{
		[Serializable]
		struct ParallaxEntry
		{
			public Material OverrideMaterial;
			public Texture2D OverrideTexture;
			public float Alpha;
			public MeshRenderer Renderer;
		}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
		[SerializeField] MeshRenderer rimRenderer;
		[SerializeField] Transform parallaxAnchor;
		[SerializeField] ParallaxEntry[] parallaxEntries;
		[SerializeField] float parallaxScalar;
		[SerializeField] AnimationCurve parallaxOffsetCurve;
		[SerializeField] AnimationCurve chromaticOffsetScaleCurve;

		[SerializeField] Transform startButtonRoot;
		[SerializeField] Transform startButtonAnchor;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null

		public Color HoloColor { set { } } // lipMesh.material.SetColor(ShaderConstants.HoloLipAdditive.LipColor, value); } }

		public Vector3 PointerViewport
		{
			set
			{
				var anchorViewport = App.V.Camera.WorldToViewportPoint(parallaxAnchor.position);
				var delta = (anchorViewport - value).NewZ(0f) * parallaxScalar;
				var deltaMagnitude = delta.magnitude;
				var chromaticAberration = chromaticOffsetScaleCurve.Evaluate(deltaMagnitude);

				delta = new Vector3(
					parallaxOffsetCurve.Evaluate(Mathf.Abs(delta.x)) * Mathf.Sign(delta.x),
					parallaxOffsetCurve.Evaluate(Mathf.Abs(delta.y)) * Mathf.Sign(delta.y),
					Mathf.Clamp01(delta.z)
				);

				var currMultiplier = 1f;
				var currZoom = 0.5f * ((Mathf.Clamp01(1f - deltaMagnitude) * 0.75f) + 0.25f);
				var currZoomScalar = deltaMagnitude;
				foreach (var entry in parallaxEntries)
				{
					entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.ChromaticOffsetScale, chromaticAberration);
					entry.Renderer.material.SetVector(ShaderConstants.HoloGamemodePortalParallax.ParallaxCoordinates, (delta * currMultiplier).NewZ(currZoom));
					currMultiplier *= 0.6f;
					//currZoom *= currZoomScalar;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();

			HoloColor = Color.white;

			foreach (var entry in parallaxEntries)
			{
				if (entry.OverrideMaterial == null) entry.Renderer.material = new Material(entry.Renderer.material);
				else entry.Renderer.material = new Material(entry.OverrideMaterial);
				if (entry.OverrideTexture != null) entry.Renderer.material.SetTexture(ShaderConstants.HoloGamemodePortalParallax.PrimaryTexture, entry.OverrideTexture);
				entry.Renderer.material.SetFloat(ShaderConstants.HoloGamemodePortalParallax.Alpha, entry.Alpha);
			}

			startButtonRoot.SetParent(startButtonAnchor, false);
			startButtonRoot.localPosition = Vector3.zero;

			rimRenderer.material = new Material(rimRenderer.material);
		}
	}

	public interface IGamemodePortalView : IView, IHoloColorView
	{
		Vector3 PointerViewport { set; }
	}
}